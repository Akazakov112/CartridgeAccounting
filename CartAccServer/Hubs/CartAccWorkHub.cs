using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CartAccLibrary.Dto;
using CartAccLibrary.Services;
using CartAccServer.Models.Infrastructure;
using CartAccServer.Models.Interfaces.Infrastructure;
using CartAccServer.Models.Interfaces.Services;
using CartAccServer.Models.Interfaces.Utility;
using CartAccServer.Models.Services;
using CartAccServer.Models.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CartAccServer.Hubs
{
    /// <summary>
    /// Хаб для работы с учетом картриджей.
    /// </summary>
    [Authorize]
    public class CartAccWorkHub : Hub
    {
        /// <summary>
        /// Объект логирования.
        /// </summary>
        private ILogger<CartAccWorkHub> Logger { get; }

        /// <summary>
        /// Конфигурация приложения.
        /// </summary>
        private IConfiguration AppConfiguration { get; }

        /// <summary>
        /// Сервис обновлений клиента.
        /// </summary>
        private IClientUpdateService ClientUpdateService { get; }

        /// <summary>
        /// Сервис работы с заблокированными документами.
        /// </summary>
        private IBlockedDocumentService BlockedDocumentService { get; }

        /// <summary>
        /// Сервис работы с данными.
        /// </summary>
        private IDataService DataService { get; }

        /// <summary>
        /// Сервис работы с подключенными пользователями.
        /// </summary>
        private IConnectedUserProvider ConnectedUser { get; }

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="cartAccContext">Контекст базы данных учета картриджей</param>
        public CartAccWorkHub(IConfiguration configuration, ILogger<CartAccWorkHub> logger, IClientUpdateService clientUpdateService,
            IBlockedDocumentService blockedDocumentService, IDataService dataService, IConnectedUserProvider provider)
        {
            AppConfiguration = configuration;
            Logger = logger;
            ClientUpdateService = clientUpdateService;
            DataService = dataService;
            BlockedDocumentService = blockedDocumentService;
            ConnectedUser = provider;
        }


        /// <summary>
        /// Подключение клиента.
        /// </summary>
        /// <returns></returns>
        public override async Task OnConnectedAsync()
        {
            // Получить логин из контекста запроса.
            string login = Context.User.Identity.Name.Split('\\')[1].Trim();
            try
            {
                // Получить пользователя.
                UserDTO authUser = login is null ? null : DataService.Users.Find(x => x.Login == login && x.Active && x.Osp.Active).FirstOrDefault();
                if (authUser is null)
                {
                    // При ошибке записать лог.
                    Logger.LogWarning($"Неудачная аутентификация, пользователь не найден. Логин - {login}");
                    // Отправить отказ авторизации.
                    await Clients.Caller.SendAsync("AccessDenied");
                }
                else
                {
                    // Получить данные пользователя по ОСП.
                    OspDataDTO ospData = DataService.GetOspData(authUser.Id, authUser.Osp.Id);
                    // Назначить текущего пользователя.
                    ospData.CurrentUser = authUser;
                    // Отправить объект данных пользователя.
                    await Clients.Caller.SendAsync("AccessAllowed", ospData);
                    // Если баланс ОСП пустой отправить сообщение.
                    if (!ospData.Balance.Any())
                    {
                        await Clients.Caller.SendAsync("AddLog", new LogMessage("В ОСП отсутствуют картриджи, добавьте поступление."));
                    }
                    // Если в ОСП отсутствуют поставщики отправить сообщение.
                    if (!ospData.Providers.Any())
                    {
                        await Clients.Caller.SendAsync("AddLog", new LogMessage("Для добавления поступлений требуются зарегистрированные поставщики."));
                    }
                    // Добавить пользователя в группу по Id ОСП.
                    await Groups.AddToGroupAsync(Context.ConnectionId, authUser.Osp.Id.ToString());
                    // Добавить в контекст пользователя полное имя.
                    Context.Items.Add("username", $"{authUser.Fullname}");
                    // Добавить подключенного пользователя в список.
                    ConnectedUser.AddUser(authUser.Id, Context.ConnectionId, authUser.Fullname, authUser.Osp.Name);
                }
            }
            catch (ValidationException valEx)
            {
                // При ошибке записать лог.
                Logger.LogWarning($"Неудачная аутентификация, {valEx.Message}. Login - {login}");
                // Отправить отказ авторизации.
                await Clients.Caller.SendAsync("AccessDenied");
            }
        }

        /// <summary>
        /// Отключение клиента.
        /// </summary>
        /// <param name="exception">Исключение</param>
        /// <returns></returns>
        public override Task OnDisconnectedAsync(Exception exception)
        {
            // Если в контексте заполнено имя пользователя клиента.
            if (Context.Items["username"] != null)
            {
                // Снять блок с документов по имени пользователя.
                BlockedDocumentService.UnsetBlock(Context.Items["username"].ToString());
            }
            ConnectedUser.RemoveUser(Context.ConnectionId);
            return base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Отправляет клиенту данные ОСП по пользователю.
        /// </summary>
        /// <param name="user">Пользователь</param>
        /// <returns></returns>
        public async Task GetOspData(UserDTO user, int ospId)
        {
            try
            {
                // Удалить пользователя из группы его ОСП.
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, user.Osp.Id.ToString());
                // Добавить пользователя в группу по названию переключаемого ОСП.
                await Groups.AddToGroupAsync(Context.ConnectionId, ospId.ToString());
                // Найти ОСП.
                OspDTO osp = DataService.Osps.Get(ospId);
                user.Osp = osp;
                // Получить данные пользователя по ОСП.
                OspDataDTO ospData = DataService.GetOspData(user.Id, ospId);
                ospData.CurrentUser = user;
                // Отправить объект данных пользователя.
                await Clients.Caller.SendAsync("UpdateOspData", ospData);
                // Отправить сообщение об успешном подключении.
                await Clients.Caller.SendAsync("AddLog", new LogMessage($"Подключено к ОСП {osp.Name}."));
            }
            catch (ValidationException valEx)
            {
                // Отправить сообщение клиенту о неудаче.
                await Clients.Caller.SendAsync("AddLog", new LogMessage($"Ошибка подключения к ОСП: {valEx.Message}.", LogMessageType.Error));
            }
        }

        /// <summary>
        /// Проверяет обновления клиента.
        /// </summary>
        /// <param name="version">Текущая версия клиента</param>
        /// <returns></returns>
        public async Task CheckUpdate(int version)
        {
            // Проверить обновления.
            try
            {
                ClientUpdateDTO update = ClientUpdateService.CheckUpdate(version);
                // Отправить обновление.
                await Clients.Caller.SendAsync("NewUpdate", update);
            }
            catch (ValidationException valEx)
            {
                // Отправить сообщение об отсутствии обновлений.
                await Clients.Caller.SendAsync("AddLog", new LogMessage(valEx.Message));
            }
            // Найти подключенного клиета и присвоить значение версии клиента.
            IConnectedUser user = ConnectedUser.ConnectedUsers.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            if (user != null)
            {
                user.ClientVersion = version;
            }
        }

        /// <summary>
        /// Открыть документ.
        /// </summary>
        /// <param name="documentId">Id документа</param>
        /// <param name="ospId">Id ОСП</param>
        /// <param name="type">Тип документа</param>
        /// <param name="user">Имя пользователя</param>
        /// <returns></returns>
        public async Task OpenDocument(int documentId, int ospId, string type, string user)
        {
            // Получить заблокированный документ.
            IBlockedDocument document = BlockedDocumentService.CheckBlock(documentId, type, ospId);
            // Если документ не найден.
            if (document is null)
            {
                // Добавить блокировку на документ.
                BlockedDocumentService.SetBlock(documentId, ospId, type, user);
                // В зависимости от типа документа отправить разрешение на клиент.
                switch (type)
                {
                    case "ExpenseDTO":
                        await Clients.Caller.SendAsync("AllowExpenseEdit");
                        break;
                    case "ReceiptDTO":
                        await Clients.Caller.SendAsync("AllowReceiptEdit");
                        break;
                    case "UserDTO":
                        await Clients.Caller.SendAsync("AllowUserEdit");
                        break;
                    case "ProviderDTO":
                        await Clients.Caller.SendAsync("AllowProviderEdit");
                        break;
                    case "EmailDTO":
                        await Clients.Caller.SendAsync("AllowEmailEdit");
                        break;
                    case "CartridgeDTO":
                        await Clients.Caller.SendAsync("AllowCartridgeEdit");
                        break;
                    case "PrinterDTO":
                        await Clients.Caller.SendAsync("AllowPrinterEdit");
                        break;
                    case "OspDTO":
                        await Clients.Caller.SendAsync("AllowOspEdit");
                        break;
                }
            }
            // Если документ найден отправить клиенту имя заблокировавшего документ пользователя.
            else
            {
                await Clients.Caller.SendAsync("DeniedEdit", document.User);
            }
        }

        /// <summary>
        /// Закрыть документ.
        /// </summary>
        /// <param name="documentId">Id документа</param>
        /// <param name="ospId">Id ОСП</param>
        /// <param name="type">Тип документа</param>
        /// <param name="user">Имя пользователя</param>
        /// <returns></returns>
        public async Task CloseDocument(int documentId, string type, int ospId = 0)
        {
            // Получить заблокированный документ.
            IBlockedDocument document = BlockedDocumentService.CheckBlock(documentId, type, ospId);
            // Если документ найден.
            if (document != null)
            {
                // Снять блокировку с документа.
                BlockedDocumentService.UnsetBlock(document);
            }
            await Task.CompletedTask;
        }


        // Юзается только на сервере, пересмотреть.
        /// <summary>
        /// Отправляет группе клиента обновление баланса.
        /// </summary>
        /// <param name="ospId">Id ОСП</param>
        /// <returns></returns>
        private async Task GetBalances(int ospId)
        {
            try
            {
                // Получить обновленный баланс по ОСП.
                ICollection<BalanceDTO> newBalances = DataService.Balance.Find(x => x.Osp.Id == ospId);
                // Отправить группе клиента обновленный баланс.
                await Clients.Group(ospId.ToString()).SendAsync("UpdateBalances", newBalances);
            }
            catch (ValidationException valEx)
            {
                // Отправить сообщение клиенту о неудаче.
                await Clients.Group(ospId.ToString()).SendAsync("AddLog", new LogMessage($"Ошибка обновления баланса: {valEx.Message}", LogMessageType.Error));
            }
        }

        /// <summary>
        /// Сохраняет результат инвентаризации картриджей.
        /// </summary>
        /// <param name="ospId">Id ОСП проведения инвентаризации.</param>
        /// <param name="editedBalances">Баланс после инвентаризации</param>
        /// <returns></returns>
        public async Task SaveInventBalance(int ospId, int userId, List<InventBalanceDTO> editedBalances)
        {
            try
            {
                // Создать объект для инвентаризации.
                var inventory = new InventoryOfBalanceService(ospId, userId, editedBalances, DataService);
                // Если по итогам инвентаризации создались корректирующие списания.
                if (inventory.Expenses.Any())
                {
                    // Отправить группе клиента добавленные в результате инвентаризации списания для списка ОСП.
                    await Clients.Group(ospId.ToString()).SendAsync("AddNewOspExpenses", inventory.Expenses);
                    // Отправить группе клиента добавленные в результате инвентаризации списания для списка пользователя.
                    await Clients.Caller.SendAsync("AddNewUserExpenses", inventory.Expenses);
                }
                // Если по итогам инвентаризации создалось корректирующее поступление.
                if (inventory.Receipt != null)
                {
                    // Отправить группе клиента добавленное в результате инвентаризации поступление.
                    await Clients.Group(ospId.ToString()).SendAsync("AddNewOspReceipt", inventory.Receipt);
                }
                // Если по итогам инвентаризации создавались документы движения картриджа.
                if (inventory.Expenses.Any() || inventory.Receipt != null)
                {
                    // Отправить группе клиента обновленный баланс.
                    await GetBalances(ospId);
                }
                // Отправить сообщение клиенту об успехе.
                await Clients.Caller.SendAsync("AddLog", new LogMessage("Инвентаризация записана."));
            }
            catch (ValidationException valEx)
            {
                // Отправить сообщение клиенту о неудаче.
                await Clients.Caller.SendAsync("AddLog", new LogMessage($"Ошибка записи инвентаризации: {valEx.InnerException.Message}", LogMessageType.Error));
            }
            catch (DbUpdateException dbuEx)
            {
                // Отправить сообщение клиенту о неудаче.
                await Clients.Caller.SendAsync("AddLog", new LogMessage($"Ошибка записи инвентаризации: {dbuEx.InnerException.Message}", LogMessageType.Error));
            }
        }

        /// <summary>
        /// Обновляет баланс в БД.
        /// </summary>
        /// <param name="editBalance">Редактированный DTO аланса</param>
        /// <returns></returns>
        public async Task UpdateBalance(BalanceDTO editBalance)
        {
            try
            {
                // Обновить данные баланса по Dto.
                DataService.Balance.Update(editBalance);
                // Отправить группе клиента обновленный баланс.
                await Clients.Group(editBalance.OspId.ToString()).SendAsync("UpdateBalance", editBalance);
                // Отправить сообщение клиенту об успехе.
                await Clients.Caller.SendAsync("AddLog", new LogMessage("Данные картриджа на балансе успешно обновлены."));
            }
            catch (ValidationException valEx)
            {
                // Отправить сообщение клиенту о неудаче.
                await Clients.Caller.SendAsync("AddLog", new LogMessage($"Ошибка обновления баланса: {valEx.Message}", LogMessageType.Error));
            }
        }


        /// <summary>
        /// Добавляет новое списание.
        /// </summary>
        /// <param name="newExpense">Новое списание с клиента</param>
        /// <returns></returns>
        public async Task AddExpense(ExpenseDTO newExpense)
        {
            try
            {
                // Добавить новое списание в бд.
                DataService.Expenses.Add(newExpense);
                // Отправить группе клиента обновленный баланс.
                await GetBalances(newExpense.OspId);
                // Найти добавленное списание в ОСП.
                ExpenseDTO addedExpense = DataService.Expenses.Find(x => x.Osp.Id == newExpense.OspId).LastOrDefault();
                // Отправить клиенту добавленное списание.
                await Clients.Caller.SendAsync("AddNewUserExpenses", new List<ExpenseDTO>() { addedExpense });
                // Отправить группе клиента добавленное списание.
                await Clients.Group(newExpense.OspId.ToString()).SendAsync("AddNewOspExpenses", new List<ExpenseDTO>() { addedExpense });
                // Отправить сообщение клиенту об успехе.
                await Clients.Caller.SendAsync("AddLog", new LogMessage("Списание добавлено."));
            }
            catch (ValidationException valEx)
            {
                // Отправить сообщение клиенту о неудаче.
                await Clients.Caller.SendAsync("AddLog", new LogMessage($"Ошибка записи: {valEx.Message}", LogMessageType.Error));
            }
        }

        /// <summary>
        /// Отправляет клиенту списание по Id.
        /// </summary>
        /// <param name="id">Id искомого списания</param>
        /// <returns></returns>
        public async Task GetExpense(int id)
        {
            try
            {
                // Получить списание в БД.
                ExpenseDTO expense = DataService.Expenses.Get(id);
                // Отправить группе клиента обновленное списание.
                await Clients.Caller.SendAsync("UpdateExpense", expense);
            }
            catch (ValidationException valEx)
            {
                // Отправить сообщение клиенту о неудаче.
                await Clients.Caller.SendAsync("AddLog", new LogMessage($"Ошибка получения списания: {valEx.Message}", LogMessageType.Error));
            }
        }

        /// <summary>
        /// Отправляет клиенту списания пользователя по Id за период.
        /// </summary>
        /// <param name="user">Пользователь</param>
        /// <param name="startPeriod">Начало периода</param>
        /// <param name="endPeriod">Конец периода</param>
        /// <returns></returns>
        public async Task GetUserExpenses(int ospId, int userId, DateTime startPeriod, DateTime endPeriod)
        {
            try
            {
                // Получить списания пользователя в ОСП за период.
                ICollection<ExpenseDTO> userExpenses = DataService.Expenses.Find(u => u.User.Id == userId && u.Osp.Id == ospId && u.Date >= startPeriod && u.Date <= endPeriod);
                // Отправить вызывающему клиенту обновление его списаний за переданный период.
                await Clients.Caller.SendAsync("UpdateUserExpenses", userExpenses);
            }
            catch (ValidationException valEx)
            {
                // Отправить сообщение клиенту о неудаче.
                await Clients.Caller.SendAsync("AddLog", new LogMessage($"Ошибка обновления списаний: {valEx.Message}", LogMessageType.Error));
            }
        }

        /// <summary>
        /// Отправляет клиенту списания ОСП за период.
        /// </summary>
        /// <param name="ospId">Id ОСП</param>
        /// <param name="startPeriod">Начало периода</param>
        /// <param name="endPeriod">Конец периода</param>
        /// <returns></returns>
        public async Task GetOspExpenses(int ospId, DateTime startPeriod, DateTime endPeriod)
        {
            try
            {
                // Получить списания в ОСП за период.
                ICollection<ExpenseDTO> ospExpenses = DataService.Expenses.Find(u => u.Osp.Id == ospId && u.Date >= startPeriod && u.Date <= endPeriod);
                // Отправить группе клиента список списаний за переданный период.
                await Clients.Caller.SendAsync("UpdateOspExpenses", ospExpenses);
            }
            catch (ValidationException valEx)
            {
                // Отправить сообщение клиенту о неудаче.
                await Clients.Caller.SendAsync("AddLog", new LogMessage($"Ошибка обновления списаний: {valEx.Message}", LogMessageType.Error));
            }
        }

        /// <summary>
        /// Обновляет списание в БД.
        /// </summary>
        /// <param name="editedExpense">Отредактированное списание</param>
        /// <returns></returns>
        public async Task UpdateExpense(ExpenseDTO editedExpense)
        {
            try
            {
                // Если списание помечено на удаление.
                if (editedExpense.Delete)
                {
                    // Найти списание в БД.
                    ExpenseDTO dbExpense = DataService.Expenses.Get(editedExpense.Id);
                    // Установить метки удаления и редактирования.
                    dbExpense.Delete = editedExpense.Delete;
                    dbExpense.Edit = true;
                    // Обновить списание в БД.
                    DataService.Expenses.Update(dbExpense);
                    // Отправить группе клиента списание с данными из БД и обновленными статусами.
                    await Clients.Group(editedExpense.OspId.ToString()).SendAsync("UpdateExpense", dbExpense);
                }
                else
                {
                    // Установить метку редактирования.
                    editedExpense.Edit = true;
                    // Обновить списание в БД.
                    DataService.Expenses.Update(editedExpense);
                    // Отправить группе клиента обновленное списание.
                    await Clients.Group(editedExpense.OspId.ToString()).SendAsync("UpdateExpense", editedExpense);
                }
                // Снять блок с документа.
                await CloseDocument(editedExpense.Id, editedExpense.GetType().Name, editedExpense.OspId);
                // Отправить группе клиента обновленный баланс.
                await GetBalances(editedExpense.OspId);
                // Отправить сообщение клиенту о сохранении.
                await Clients.Caller.SendAsync("AddLog", new LogMessage($"Списание №{editedExpense.Number} обновлено."));
                // Отправить сообщение остальным участникам группы об изменении списания.
                await Clients.OthersInGroup(editedExpense.OspId.ToString())
                    .SendAsync("AddLog", new LogMessage($"Пользователь {Context.Items["username"]} изменил списание №{editedExpense.Number}"));
            }
            catch (ValidationException valEx)
            {
                // Отправить сообщение клиенту о неудаче.
                await Clients.Caller.SendAsync("AddLog", new LogMessage($"Ошибка обновления списания: {valEx.Message}", LogMessageType.Error));
            }
        }


        /// <summary>
        /// Добавляет новое поступление.
        /// </summary>
        /// <param name="newReceipt">Новое поступление с клиента</param>
        /// <returns></returns>
        public async Task AddReceipt(ReceiptDTO newReceipt)
        {
            try
            {
                // Добавить новое поступление в бд.
                DataService.Receipts.Add(newReceipt);
                // Отправить группе клиента обновленный баланс.
                await GetBalances(newReceipt.OspId);
                // Найти добавленное поступление в ОСП.
                ReceiptDTO addedReceipt = DataService.Receipts.Find(x => x.Osp.Id == newReceipt.OspId).LastOrDefault();
                // Отправить группе клиента добавленное поступление.
                await Clients.Group(newReceipt.OspId.ToString()).SendAsync("AddNewOspReceipt", addedReceipt);
                // Отправить сообщение остальным участникам группы о добавлении поступления.
                await Clients.OthersInGroup(newReceipt.OspId.ToString()).SendAsync("AddLog", new LogMessage($"Пользователь {Context.Items["username"]} добавил поступление картриджей."));
                // Отправить сообщение клиенту об успехе.
                await Clients.Caller.SendAsync("AddLog", new LogMessage("Поступление добавлено."));
            }
            catch (ValidationException valEx)
            {
                // Отправить сообщение клиенту о неудаче.
                await Clients.Caller.SendAsync("AddLog", new LogMessage($"Ошибка записи: {valEx.Message}", LogMessageType.Error));
            }
        }

        /// <summary>
        /// Отправляет клиенту поступление по Id.
        /// </summary>
        /// <param name="id">Id искомого поступления</param>
        /// <returns></returns>
        public async Task GetReceipt(int id)
        {
            try
            {
                // Получить поступление в БД.
                ReceiptDTO receipt = DataService.Receipts.Get(id);
                // Отправить клиенту поступление.
                await Clients.Caller.SendAsync("UpdateReceipt", receipt);
            }
            catch (ValidationException valEx)
            {
                // Отправить сообщение клиенту о неудаче.
                await Clients.Caller.SendAsync("AddLog", new LogMessage($"Ошибка получения поступления: {valEx.Message}", LogMessageType.Error));
            }
        }

        /// <summary>
        /// Отправляет клиенту поступления ОСП за период.
        /// </summary>
        /// <param name="ospId">Id ОСП</param>
        /// <param name="startPeriod">Начало периода</param>
        /// <param name="endPeriod">Конец периода</param>
        /// <returns></returns>
        public async Task GetOspReceipts(int ospId, DateTime startPeriod, DateTime endPeriod)
        {
            try
            {
                // Получить поступления в ОСП за период.
                ICollection<ReceiptDTO> ospReceipts = DataService.Receipts.Find(u => u.Osp.Id == ospId && u.Date >= startPeriod && u.Date <= endPeriod);
                // Отправить группе клиента список поступлений за переданный период.
                await Clients.Caller.SendAsync("UpdateOspReceipts", ospReceipts);
            }
            catch (ValidationException valEx)
            {
                // Отправить сообщение клиенту о неудаче.
                await Clients.Caller.SendAsync("AddLog", new LogMessage($"Ошибка обновления поступлений: {valEx.Message}", LogMessageType.Error));
            }
        }

        /// <summary>
        /// Обновляет поступление в БД.
        /// </summary>
        /// <param name="editedReceipt">Отредактированное поступление</param>
        /// <returns></returns>
        public async Task UpdateReceipt(ReceiptDTO editedReceipt)
        {
            try
            {
                // Если поступление помечено на удаление.
                if (editedReceipt.Delete)
                {
                    // Найти поступление в БД.
                    ReceiptDTO dbReceipt = DataService.Receipts.Get(editedReceipt.Id);
                    // Установить метки удаления и редактирования.
                    dbReceipt.Delete = editedReceipt.Delete;
                    dbReceipt.Edit = true;
                    // Обновить списание в БД.
                    DataService.Receipts.Update(dbReceipt);
                    // Отправить группе клиента поступление с данными из БД и обновленными статусами.
                    await Clients.Group(editedReceipt.OspId.ToString()).SendAsync("UpdateReceipt", dbReceipt);
                }
                else
                {
                    // Установить метку редактирования.
                    editedReceipt.Edit = true;
                    // Обновить поступление в БД.
                    DataService.Receipts.Update(editedReceipt);
                    // Отправить группе клиента обновленное поступление.
                    await Clients.Group(editedReceipt.OspId.ToString()).SendAsync("UpdateReceipt", editedReceipt);
                }
                // Снять блок с документа.
                await CloseDocument(editedReceipt.Id, editedReceipt.GetType().Name, editedReceipt.OspId);
                // Отправить группе клиента обновленный баланс.
                await GetBalances(editedReceipt.OspId);
                // Отправить сообщение клиенту о сохранении.
                await Clients.Caller.SendAsync("AddLog", new LogMessage($"Поступление №{editedReceipt.Number} обновлено."));
                // Отправить сообщение остальным участникам группы об изменении поступления.
                await Clients.OthersInGroup(editedReceipt.OspId.ToString())
                    .SendAsync("AddLog", new LogMessage($"Пользователь {Context.Items["username"]} изменил поступление №{editedReceipt.Number}"));
            }
            catch (ValidationException valEx)
            {
                // Отправить сообщение клиенту о неудаче.
                await Clients.Caller.SendAsync("AddLog", new LogMessage($"Ошибка обновления поступления: {valEx.Message}", LogMessageType.Error));
            }
        }


        /// <summary>
        /// Добавляет нового поставщика.
        /// </summary>
        /// <param name="newProvider">Новый поставщик</param>
        /// <returns></returns>
        public async Task AddProvider(ProviderDTO newProvider)
        {
            try
            {
                // Добавить нового поставщика в бд.
                DataService.Providers.Add(newProvider);
                // Найти добавленного поставщика в БД.
                ProviderDTO addedProvider = DataService.Providers.Find(x => x.Osp.Id == newProvider.OspId).LastOrDefault();
                // Отправить группе клиента добавленного поставщика.
                await Clients.Group(newProvider.OspId.ToString()).SendAsync("AddNewProvider", addedProvider);
                // Отправить сообщение остальным участникам группы о добавлении поступления.
                await Clients.OthersInGroup(newProvider.OspId.ToString()).SendAsync("AddLog",
                    new LogMessage($"Пользователь {Context.Items["username"]} добавил поставщика {addedProvider.Name}."));
                // Отправить сообщение клиенту об успехе.
                await Clients.Caller.SendAsync("AddLog", new LogMessage("Поставщик добавлен."));
            }
            catch (ValidationException valEx)
            {
                // Отправить сообщение клиенту о неудаче.
                await Clients.Caller.SendAsync("AddLog", new LogMessage($"Ошибка записи: {valEx.Message}", LogMessageType.Error));
            }
        }

        /// <summary>
        /// Отправляет клиенту поставщика по Id.
        /// </summary>
        /// <param name="id">Id поставщика</param>
        /// <returns></returns>
        public async Task GetProvider(int id)
        {
            try
            {
                // Получить поставщика в БД.
                ProviderDTO provider = DataService.Providers.Get(id);
                // Отправить клиенту поставщика.
                await Clients.Caller.SendAsync("UpdateProvider", provider);
            }
            catch (ValidationException valEx)
            {
                // Отправить сообщение клиенту о неудаче.
                await Clients.Caller.SendAsync("AddLog", new LogMessage($"Ошибка получения поставщика: {valEx.Message}", LogMessageType.Error));
            }
        }

        /// <summary>
        /// Обновляет поставщика в БД.
        /// </summary>
        /// <param name="editedProvider">Отредактированный поставщик</param>
        /// <returns></returns>
        public async Task UpdateProvider(ProviderDTO editedProvider)
        {
            try
            {
                // Если поставщик помечен как актуальный.
                if (editedProvider.Active)
                {
                    // Обновить поставщика в БД.
                    DataService.Providers.Update(editedProvider);
                    // Отправить группе клиента обновленного поставщика.
                    await Clients.Group(editedProvider.OspId.ToString()).SendAsync("UpdateProvider", editedProvider);
                }
                else
                {
                    // Найти поставщика в БД.
                    ProviderDTO dbProvider = DataService.Providers.Get(editedProvider.Id);
                    // Установить метку актуальности.
                    dbProvider.Active = editedProvider.Active;
                    // Обновить поставщика в БД.
                    DataService.Providers.Update(dbProvider);
                    // Отправить группе клиента поступление с данными из БД и обновленным статусом.
                    await Clients.Group(editedProvider.OspId.ToString()).SendAsync("UpdateProvider", dbProvider);
                }
                // Снять блок с документа.
                await CloseDocument(editedProvider.Id, editedProvider.GetType().Name, editedProvider.OspId);
                // Отправить сообщение клиенту о сохранении.
                await Clients.Caller.SendAsync("AddLog", new LogMessage($"Данные поставщика {editedProvider.Name} обновлены."));
                // Отправить сообщение остальным участникам группы об изменении поставщика.
                await Clients.OthersInGroup(editedProvider.OspId.ToString())
                    .SendAsync("AddLog", new LogMessage($"Пользователь {Context.Items["username"]} изменил данные поставщика {editedProvider.Name}"));
            }
            catch (ValidationException valEx)
            {
                // Отправить сообщение клиенту о неудаче.
                await Clients.Caller.SendAsync("AddLog", new LogMessage($"Ошибка обновления поставщика: {valEx.Message}", LogMessageType.Error));
            }
        }


        /// <summary>
        /// Добавляет новую почту.
        /// </summary>
        /// <param name="newCartridge">Новая почта</param>
        /// <returns></returns>
        public async Task AddEmail(EmailDTO newEmail)
        {
            try
            {
                // Добавить новую почту в бд.
                DataService.Emails.Add(newEmail);
                // Найти добавленную почту в БД.
                EmailDTO addedEmail = DataService.Emails.Find(x => x.Osp.Id == newEmail.OspId).LastOrDefault();
                // Отправить группе клиента добавленную почту.
                await Clients.Group(newEmail.OspId.ToString()).SendAsync("AddNewEmail", addedEmail);
                // Отправить сообщение остальным участникам группы о добавлении почты.
                await Clients.OthersInGroup(newEmail.OspId.ToString()).SendAsync("AddLog",
                    new LogMessage($"Пользователь {Context.Items["username"]} добавил новую запись электронной почты {addedEmail.Address}."));
                // Отправить сообщение клиенту об успехе.
                await Clients.Caller.SendAsync("AddLog", new LogMessage("Электронная почта добавлена."));
            }
            catch (ValidationException valEx)
            {
                // Отправить сообщение клиенту о неудаче.
                await Clients.Caller.SendAsync("AddLog", new LogMessage($"Ошибка записи: {valEx.Message}", LogMessageType.Error));
            }
        }

        /// <summary>
        /// Отправляет клиенту электронную почту по Id.
        /// </summary>
        /// <param name="id">Id почты</param>
        /// <returns></returns>
        public async Task GetEmail(int id)
        {
            try
            {
                // Получить почту в БД.
                EmailDTO email = DataService.Emails.Get(id);
                // Отправить клиенту почту.
                await Clients.Caller.SendAsync("UpdateEmail", email);
            }
            catch (ValidationException valEx)
            {
                // Отправить сообщение клиенту о неудаче.
                await Clients.Caller.SendAsync("AddLog", new LogMessage($"Ошибка получения почты: {valEx.Message}", LogMessageType.Error));
            }
        }

        /// <summary>
        /// Обновляет почту в БД.
        /// </summary>
        /// <param name="editedEmail">Отредактированная запись эл. почты</param>
        /// <returns></returns>
        public async Task UpdateEmail(EmailDTO editedEmail)
        {
            try
            {
                // Если почта помечена как актуальная.
                if (editedEmail.Active)
                {
                    // Обновить почту в БД.
                    DataService.Emails.Update(editedEmail);
                    // Отправить группе клиента обновленную почту.
                    await Clients.Group(editedEmail.OspId.ToString()).SendAsync("UpdateEmail", editedEmail);
                }
                else
                {
                    // Найти почту в БД.
                    EmailDTO dbEmail = DataService.Emails.Get(editedEmail.Id);
                    // Установить метку актуальности.
                    dbEmail.Active = editedEmail.Active;
                    // Обновить почту в БД.
                    DataService.Emails.Update(dbEmail);
                    // Отправить группе клиента почту с данными из БД и обновленным статусом.
                    await Clients.Group(editedEmail.OspId.ToString()).SendAsync("UpdateEmail", dbEmail);
                }
                // Снять блок с документа.
                await CloseDocument(editedEmail.Id, editedEmail.GetType().Name, editedEmail.OspId);
                // Отправить сообщение клиенту о сохранении.
                await Clients.Caller.SendAsync("AddLog", new LogMessage($"Данные электронной почты {editedEmail.Address} обновлены."));
                // Отправить сообщение остальным участникам группы об изменении почты.
                await Clients.OthersInGroup(editedEmail.OspId.ToString())
                    .SendAsync("AddLog", new LogMessage($"Пользователь {Context.Items["username"]} изменил запись электронной почты {editedEmail.Address}"));
            }
            catch (ValidationException valEx)
            {
                // Отправить сообщение клиенту о неудаче.
                await Clients.Caller.SendAsync("AddLog", new LogMessage($"Ошибка обновления записи электронной почты: {valEx.Message}", LogMessageType.Error));
            }
        }


        /// <summary>
        /// Добавляет новый картридж.
        /// </summary>
        /// <param name="newCartridge">Новый картридж</param>
        /// <returns></returns>
        public async Task AddCartridge(CartridgeDTO newCartridge)
        {
            try
            {
                // Добавить новый картридж в бд.
                DataService.Cartridges.Add(newCartridge);
                // Найти добавленный картридж в БД.
                CartridgeDTO addedCartridge = DataService.Cartridges.GetAll().LastOrDefault();
                // Отправить всем клиентам добавленный картридж.
                await Clients.All.SendAsync("AddNewCartridge", addedCartridge);
                // Отправить всем клиентам словарь принтеров.
                await Clients.All.SendAsync("UpdatePrinters", DataService.Printers.GetAll());
                // Отправить сообщение всем остальным клиентам о добавлении картриджа.
                await Clients.Others.SendAsync("AddLog", new LogMessage($"Пользователь {Context.Items["username"]} добавил новый картридж модели {addedCartridge.Model}."));
                // Отправить сообщение клиенту об успехе.
                await Clients.Caller.SendAsync("AddLog", new LogMessage("Картридж добавлен."));
            }
            catch (ValidationException valEx)
            {
                // Отправить сообщение клиенту о неудаче.
                await Clients.Caller.SendAsync("AddLog", new LogMessage($"Ошибка записи: {valEx.Message}", LogMessageType.Error));
            }
        }

        /// <summary>
        /// Отправляет клиенту картридж по Id.
        /// </summary>
        /// <param name="id">Id картриджа</param>
        /// <returns></returns>
        public async Task GetCartridge(int id)
        {
            try
            {
                // Получить картридж в БД.
                CartridgeDTO cartridge = DataService.Cartridges.Get(id);
                // Отправить клиенту картридж.
                await Clients.Caller.SendAsync("UpdateCartridge", cartridge);
            }
            catch (ValidationException valEx)
            {
                // Отправить сообщение клиенту о неудаче.
                await Clients.Caller.SendAsync("AddLog", new LogMessage($"Ошибка получения картриджа: {valEx.Message}", LogMessageType.Error));
            }
        }

        /// <summary>
        /// Обновляет картридж в БД.
        /// </summary>
        /// <param name="editedCartridge">Отредактированная запись картриджа</param>
        /// <returns></returns>
        public async Task UpdateCartridge(CartridgeDTO editedCartridge)
        {
            try
            {
                // Если картридж помечен как используемый.
                if (editedCartridge.InUse)
                {
                    // Обновить картридж в БД.
                    DataService.Cartridges.Update(editedCartridge);
                    // Отправить всем клиентам обновленный картридж.
                    await Clients.All.SendAsync("UpdateCartridge", editedCartridge);
                    // Отправить всем клиентам словарь принтеров.
                    await Clients.All.SendAsync("UpdatePrinters", DataService.Printers.GetAll());
                }
                else
                {
                    // Найти картридж в БД.
                    CartridgeDTO dbCartridge = DataService.Cartridges.Get(editedCartridge.Id);
                    // Установить метку использования.
                    dbCartridge.InUse = editedCartridge.InUse;
                    // Обновить картридж в БД.
                    DataService.Cartridges.Update(dbCartridge);
                    // Отправить всем клиентам картридж с данными из БД и обновленным статусом.
                    await Clients.All.SendAsync("UpdateCartridge", dbCartridge);
                }
                // Снять блок с документа.
                await CloseDocument(editedCartridge.Id, editedCartridge.GetType().Name);
                // Отправить сообщение клиенту о сохранении.
                await Clients.Caller.SendAsync("AddLog", new LogMessage($"Данные картриджа модели {editedCartridge.Model} обновлены."));
                // Отправить сообщение всем остальным клиентам об изменении картриджа.
                await Clients.Others.SendAsync("AddLog",
                    new LogMessage($"Пользователь {Context.Items["username"]} изменил данные картриджа модели {editedCartridge.Model}"));
            }
            catch (ValidationException valEx)
            {
                // Отправить сообщение клиенту о неудаче.
                await Clients.Caller.SendAsync("AddLog", new LogMessage($"Ошибка обновления данных картриджа: {valEx.Message}", LogMessageType.Error));
            }
        }


        /// <summary>
        /// Добавляет новый принтер.
        /// </summary>
        /// <param name="newPrinter">Новый принтер</param>
        /// <returns></returns>
        public async Task AddPrinter(PrinterDTO newPrinter)
        {
            try
            {
                // Добавить новый принтер в бд.
                DataService.Printers.Add(newPrinter);
                // Найти добавленный принтер в БД.
                PrinterDTO addedPrinter = DataService.Printers.GetAll().LastOrDefault();
                // Отправить всем клиентам добавленный принтер.
                await Clients.All.SendAsync("AddNewPrinter", addedPrinter);
                // Отправить всем клиентам словарь картриджей.
                await Clients.All.SendAsync("UpdateCartridges", DataService.Cartridges.GetAll());
                // Отправить сообщение всем остальным клиентам о добавлении принтера.
                await Clients.Others.SendAsync("AddLog", new LogMessage($"Пользователь {Context.Items["username"]} добавил новый принтер модели {addedPrinter.Model}."));
                // Отправить сообщение клиенту об успехе.
                await Clients.Caller.SendAsync("AddLog", new LogMessage("Принтер добавлен."));
            }
            catch (ValidationException valEx)
            {
                // Отправить сообщение клиенту о неудаче.
                await Clients.Caller.SendAsync("AddLog", new LogMessage($"Ошибка записи: {valEx.Message}", LogMessageType.Error));
            }
        }

        /// <summary>
        /// Отправляет клиенту принтер по Id.
        /// </summary>
        /// <param name="id">Id принтера</param>
        /// <returns></returns>
        public async Task GetPrinter(int id)
        {
            try
            {
                // Получить принтер в БД.
                PrinterDTO printer = DataService.Printers.Get(id);
                // Отправить клиенту принтер.
                await Clients.Caller.SendAsync("UpdatePrinter", printer);
            }
            catch (ValidationException valEx)
            {
                // Отправить сообщение клиенту о неудаче.
                await Clients.Caller.SendAsync("AddLog", new LogMessage($"Ошибка получения принтера: {valEx.Message}", LogMessageType.Error));
            }
        }

        /// <summary>
        /// Обновляет принтер в БД.
        /// </summary>
        /// <param name="editedPrinter">Отредактированная запись принтера</param>
        /// <returns></returns>
        public async Task UpdatePrinter(PrinterDTO editedPrinter)
        {
            try
            {
                // Если принтер помечен как используемый.
                if (editedPrinter.InUse)
                {
                    // Обновить принтер в БД.
                    DataService.Printers.Update(editedPrinter);
                    // Отправить всем клиентам обновленный принтер.
                    await Clients.All.SendAsync("UpdatePrinter", editedPrinter);
                    // Отправить всем клиентам словарь картриджей.
                    await Clients.All.SendAsync("UpdateCartridges", DataService.Cartridges.GetAll());
                }
                else
                {
                    // Найти принтер в БД.
                    PrinterDTO dbPrinter = DataService.Printers.Get(editedPrinter.Id);
                    // Установить метку использования.
                    dbPrinter.InUse = editedPrinter.InUse;
                    // Обновить принтер в БД.
                    DataService.Printers.Update(dbPrinter);
                    // Отправить всем клиентам принтер с данными из БД и обновленным статусом.
                    await Clients.All.SendAsync("UpdatePrinter", dbPrinter);
                }
                // Снять блок с документа.
                await CloseDocument(editedPrinter.Id, editedPrinter.GetType().Name);
                // Отправить сообщение клиенту о сохранении.
                await Clients.Caller.SendAsync("AddLog", new LogMessage($"Данные принтера модели {editedPrinter.Model} обновлены."));
                // Отправить сообщение всем остальным клиентам об изменении принтера.
                await Clients.Others.SendAsync("AddLog",
                    new LogMessage($"Пользователь {Context.Items["username"]} изменил данные принтера модели {editedPrinter.Model}"));
            }
            catch (ValidationException valEx)
            {
                // Отправить сообщение клиенту о неудаче.
                await Clients.Caller.SendAsync("AddLog", new LogMessage($"Ошибка обновления данных принтера: {valEx.Message}", LogMessageType.Error));
            }
        }


        /// <summary>
        /// Добавляет новое ОСП.
        /// </summary>
        /// <param name="newOsp">Новое ОСП</param>
        /// <returns></returns>
        public async Task AddOsp(OspDTO newOsp)
        {
            try
            {
                // Добавить новое ОСП в бд.
                DataService.Osps.Add(newOsp);
                // Найти добавленное ОСП в БД.
                OspDTO addedOsp = DataService.Osps.GetAll().LastOrDefault();
                // Отправить всем клиентам добавленное ОСП.
                await Clients.All.SendAsync("AddNewOsp", addedOsp);
                // Отправить сообщение всем остальным клиентам о добавлении ОСП.
                await Clients.Others.SendAsync("AddLog", new LogMessage($"Пользователь {Context.Items["username"]} добавил новое ОСП {addedOsp.Name}."));
                // Отправить сообщение клиенту об успехе.
                await Clients.Caller.SendAsync("AddLog", new LogMessage("ОСП добавлено. Для начала работы необходимо создать или назначить пользователя в новое ОСП."));
            }
            catch (ValidationException valEx)
            {
                // Отправить сообщение клиенту о неудаче.
                await Clients.Caller.SendAsync("AddLog", new LogMessage($"Ошибка записи: {valEx.Message}", LogMessageType.Error));
            }
        }

        /// <summary>
        /// Отправляет клиенту ОСП по Id.
        /// </summary>
        /// <param name="id">Id ОСП</param>
        /// <returns></returns>
        public async Task GetOsp(int id)
        {
            try
            {
                // Получить ОСП в БД.
                OspDTO osp = DataService.Osps.Get(id);
                // Отправить клиенту ОСП.
                await Clients.Caller.SendAsync("UpdateOsp", osp);
            }
            catch (ValidationException valEx)
            {
                // Отправить сообщение клиенту о неудаче.
                await Clients.Caller.SendAsync("AddLog", new LogMessage($"Ошибка получения ОСП: {valEx.Message}", LogMessageType.Error));
            }
        }

        /// <summary>
        /// Обновляет ОСП в БД.
        /// </summary>
        /// <param name="editedOsp">Отредактированная запись ОСП</param>
        /// <returns></returns>
        public async Task UpdateOsp(OspDTO editedOsp)
        {
            try
            {
                // Если ОСП помечено как подключенное.
                if (editedOsp.Active)
                {
                    // Обновить ОСП в БД.
                    DataService.Osps.Update(editedOsp);
                    // Отправить всем клиентам обновленнjt ОСП.
                    await Clients.All.SendAsync("UpdateOsp", editedOsp);
                }
                else
                {
                    // Найти ОСП в БД.
                    OspDTO dbOsp = DataService.Osps.Get(editedOsp.Id);
                    // Установить метку актуальности.
                    dbOsp.Active = editedOsp.Active;
                    // Обновить ОСП в БД.
                    DataService.Osps.Update(dbOsp);
                    // Отправить всем клиентам ОСП с данными из БД и обновленным статусом.
                    await Clients.All.SendAsync("UpdateOsp", dbOsp);
                }
                // Снять блок с документа.
                await CloseDocument(editedOsp.Id, editedOsp.GetType().Name);
                // Отправить сообщение клиенту о сохранении.
                await Clients.Caller.SendAsync("AddLog", new LogMessage($"Данные ОСП {editedOsp.Name} обновлены."));
                // Отправить сообщение всем остальным клиентам об изменении ОСП.
                await Clients.Others.SendAsync("AddLog",
                    new LogMessage($"Пользователь {Context.Items["username"]} изменил данные ОСП {editedOsp.Name}"));
            }
            catch (ValidationException valEx)
            {
                // Отправить сообщение клиенту о неудаче.
                await Clients.Caller.SendAsync("AddLog", new LogMessage($"Ошибка обновления данных ОСП: {valEx.Message}", LogMessageType.Error));
            }
        }


        /// <summary>
        /// Добавляет нового пользователя.
        /// </summary>
        /// <param name="newUser">Новый пользователь</param>
        /// <returns></returns>
        public async Task AddUser(UserDTO newUser)
        {
            try
            {
                // Добавить нового пользователя в бд.
                DataService.Users.Add(newUser);
                // Найти добавленного пользователя в БД.
                UserDTO addedUser = DataService.Users.GetAll().LastOrDefault();
                // Отправить всем клиентам добавленного пользователя.
                await Clients.All.SendAsync("AddNewUser", addedUser);
                // Отправить сообщение клиенту об успехе.
                await Clients.Caller.SendAsync("AddLog", new LogMessage("Пользователь добавлен."));
            }
            catch (ValidationException valEx)
            {
                // Отправить сообщение клиенту о неудаче.
                await Clients.Caller.SendAsync("AddLog", new LogMessage($"Ошибка записи: {valEx.Message}", LogMessageType.Error));
            }
        }

        /// <summary>
        /// Отправляет клиенту пользователя по Id.
        /// </summary>
        /// <param name="id">Id пользователя</param>
        /// <returns></returns>
        public async Task GetUser(int id)
        {
            try
            {
                // Получить пользователя в БД.
                UserDTO user = DataService.Users.Get(id);
                // Отправить клиенту пользователя.
                await Clients.Caller.SendAsync("UpdateUser", user);
            }
            catch (ValidationException valEx)
            {
                // Отправить сообщение клиенту о неудаче.
                await Clients.Caller.SendAsync("AddLog", new LogMessage($"Ошибка получения пользователя: {valEx.Message}", LogMessageType.Error));
            }
        }

        /// <summary>
        /// Обновляет пользователя в БД.
        /// </summary>
        /// <param name="editedUser">Отредактированная запись пользователя</param>
        /// <returns></returns>
        public async Task UpdateUser(UserDTO editedUser)
        {
            try
            {
                // Если пользователь помечен как имеющий доступ.
                if (editedUser.Active)
                {
                    // Обновить пользователя в БД.
                    DataService.Users.Update(editedUser);
                    // Отправить всем клиентам обновленного пользователя.
                    await Clients.All.SendAsync("UpdateUser", editedUser);
                }
                else
                {
                    // Найти пользователя в БД.
                    UserDTO dbUser = DataService.Users.Get(editedUser.Id);
                    // Установить метку актуальности.
                    dbUser.Active = editedUser.Active;
                    // Обновить пользователя в БД.
                    DataService.Users.Update(dbUser);
                    // Отправить всем клиентам пользователя с данными из БД и обновленным статусом.
                    await Clients.All.SendAsync("UpdateUser", dbUser);
                }
                // Снять блок с документа.
                await CloseDocument(editedUser.Id, editedUser.GetType().Name, editedUser.Osp.Id);
                // Отправить сообщение клиенту о сохранении.
                await Clients.Caller.SendAsync("AddLog", new LogMessage($"Данные пользователя {editedUser.Fullname} обновлены."));
            }
            catch (ValidationException valEx)
            {
                // Отправить сообщение клиенту о неудаче.
                await Clients.Caller.SendAsync("AddLog", new LogMessage($"Ошибка обновления данных пользователя: {valEx.Message}", LogMessageType.Error));
            }
        }

        /// <summary>
        /// Отправляет список пользователей из Active Directory по Ф.И.О. пользователя.
        /// </summary>
        /// <param name="username">Ф.И.О. пользователя</param>
        /// <returns></returns>
        public async Task GetADUsers(string username)
        {
            try
            {
                // Создать объект для поиска.
                var searcher = new ADUserSearcher(AppConfiguration);
                // Получить пользователей в АД.
                List<UserDTO> users = await searcher.FindAsync(username);
                // Отправить клиенту пользователя.
                await Clients.Caller.SendAsync("GetADUsers", users);
            }
            catch (ValidationException valEx)
            {
                // Отправить сообщение клиенту о неудаче.
                await Clients.Caller.SendAsync("AddLog", new LogMessage($"Ошибка получения пользователей: {valEx.Message}", LogMessageType.Error));
            }
        }


        /// <summary>
        /// Отправляет список отчетов ОСП по движению картриджей.
        /// </summary>
        /// <param name="osps">Список ОСП</param>
        /// <param name="start">Дата начала периода отчета</param>
        /// <param name="end">Дата окончания периода отчета</param>
        /// <param name="actualCartsDays">Дни актуальности картриджа</param>
        /// <returns></returns>
        public async Task GetMotionReports(OspDTO[] osps, DateTime start, DateTime end, int actualCartsDays)
        {

            // Коллекция отчетов по ОСП.
            List<MotionReport> reports = new List<MotionReport>();
            // Перебираем переданные ОСП.
            foreach (var osp in osps)
            {
                // Построитель отчетов.
                MotionReportBuilder reportBuilder = new MotionReportBuilder(DataService, osp, start, end, actualCartsDays);
                try
                {
                    // Создать отчет по ОСП.
                    MotionReport report = (MotionReport)reportBuilder.Create();
                    // Добавить в коллекцию отчетов к возврату.
                    reports.Add(report);

                }
                catch (ValidationException valEx)
                {
                    // Отправить сообщение клиенту о неудаче.
                    await Clients.Caller.SendAsync("AddLog", new LogMessage(valEx.Message, LogMessageType.Warning));
                }
            }
            // Отправить клиенту пользователя.
            await Clients.Caller.SendAsync("UpdateReports", reports);
        }
    }
}
