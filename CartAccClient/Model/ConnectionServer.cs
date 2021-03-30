using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using CartAccClient.View;
using CartAccClient.ViewModel;
using CartAccLibrary.Dto;
using CartAccLibrary.Services;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;

namespace CartAccClient.Model
{
    /// <summary>
    /// Класс подключения к серверу SignalR.
    /// </summary>
    class ConnectionServer : BaseVm
    {
        private bool status;
        private LogMessage selectedLog;

        /// <summary>
        /// Статус подключения.
        /// </summary>
        public bool Status
        {
            get { return status; }
            private set { status = value; RaisePropertyChanged(nameof(Status)); }
        }

        /// <summary>
        /// Выбранное лог сообщение.
        /// </summary>
        public LogMessage SelectedLog
        {
            get { return selectedLog; }
            set { selectedLog = value; RaisePropertyChanged(nameof(SelectedLog)); }
        }

        /// <summary>
        /// Лог сообщения.
        /// </summary>
        public ObservableCollection<LogMessage> Log { get; private set; }

        /// <summary>
        /// Объект подключения.
        /// </summary>
        public HubConnection Connection { get; private set; }

        /// <summary>
        /// Конструктор со строкой подключения.
        /// </summary>
        /// <param name="connectionString"></param>
        private ConnectionServer(string connectionString)
        {
            InitConnect(connectionString);
        }


        /// <summary>
        /// Изменить подключение.
        /// </summary>
        /// <param name="connectionString">Строка подключения</param>
        public void ChangeServer(string connectionString)
        {
            InitConnect(connectionString);
        }

        /// <summary>
        /// Очищает лог.
        /// </summary>
        public void ClearLog()
        {
            Log.Clear();
        }

        /// <summary>
        /// Инициирует объект подключения.
        /// </summary>
        /// <param name="connectionString"></param>
        private void InitConnect(string connectionString)
        {
            Connection = new HubConnectionBuilder()
            .WithAutomaticReconnect()
            .WithUrl(connectionString, option => 
            { 
                option.UseDefaultCredentials = true;
            })
            .AddNewtonsoftJsonProtocol()
            .Build();
            Status = true;
            Log = new ObservableCollection<LogMessage>();
            Connection.Closed += Connection_Closed;
            Connection.Reconnected += Connection_Reconnected;
            Connection.Reconnecting += Connection_Reconnecting;

            // Обработчик вызова закрытия программы.
            Connection.On("CloseApp", () => Application.Current.Shutdown());
            // Обработчик вызова сообщения от сервера.
            Connection.On<string>("Alert", (message) => Alert.Show(message, "Сообщение от администратора.", MessageBoxButton.OK));
            // Обработчик вызова запрет редактирования документа списания.
            Connection.On<string>("DeniedEdit", (username) => Alert.Show($"Документ заблокирован пользователем:\n{username}"));
            // Обработчик вызова открытия доступа.
            Connection.On<OspDataDTO>("AccessAllowed", async (userData) =>
            {
                // Выполнить запрос обновления клиента на сервер.
                await Connection.InvokeAsync("CheckUpdate", JsonFileAppConfig.GetAppBuild());
                // Инициализировать данные по ОСП.
                AppData.Init(userData);
                // Открыть главное окно программы.
                var window = new WorkspaceForm();
                window.Show();
                // Закрыть окно запуска.
                Application.Current.Windows[0].Close();
            });
            // Обработчик вызова проверки обновления.
            Connection.On("CheckUpdate", async () =>
            {
                // Выполнить запрос обновления клиента на сервер.
                await Connection.InvokeAsync("CheckUpdate", JsonFileAppConfig.GetAppBuild());
            });
            // Обработчик вызова получения обновления.
            Connection.On<ClientUpdateDTO>("NewUpdate", update =>
            {
                var window = new UpdateAppForm() { DataContext = new UpdateAppVm(update) };
                window.ShowDialog();
            });
            // Обработчик вызова добавления лога.
            Connection.On<LogMessage>("AddLog", (message) =>
            {
                Application.Current.Dispatcher.Invoke(() => Log.Add(message));
                SelectedLog = message;
            });

            // Обработчик вызова получения данных ОСП.
            Connection.On<OspDataDTO>("UpdateOspData", (newOspData) => 
            { 
                AppData.Data.UserData.UpdateAll(newOspData);
                AppData.Data.UserData.CurrentUser.PropChanged += AppData.CurrentUser_PropChanged;
            });
            // Обработчик вызова обновления баланса.
            Connection.On<IEnumerable<BalanceDTO>>("UpdateBalances", (newBalance) => AppData.Data.UserData.UpdateBalances(newBalance));
            // Обработчик вызова обновления списаний пользователя.
            Connection.On<IEnumerable<ExpenseDTO>>("UpdateUserExpenses", (newUserExpenses) => AppData.Data.UserData.UpdateUserExpenses(newUserExpenses));
            // Обработчик вызова обновления списаний в ОСП.
            Connection.On<IEnumerable<ExpenseDTO>>("UpdateOspExpenses", (newOspExpenses) => AppData.Data.UserData.UpdateExpenses(newOspExpenses));
            // Обработчик вызова обновления поступлений в ОСП.
            Connection.On<IEnumerable<ReceiptDTO>>("UpdateOspReceipts", (newReceipts) => AppData.Data.UserData.UpdateReceipts(newReceipts));
            // Обработчик вызова обновления принтеров в словаре.
            Connection.On<IEnumerable<PrinterDTO>>("UpdatePrinters", (newAllPrinters) => AppData.Data.UserData.UpdatePrinters(newAllPrinters));
            // Обработчик вызова обновления картриджей в словаре.
            Connection.On<IEnumerable<CartridgeDTO>>("UpdateCartridges", (newAllCartridges) => AppData.Data.UserData.UpdateCartridges(newAllCartridges));

            // Обработчик вызова обновления одного баланса.
            Connection.On<BalanceDTO>("UpdateBalance", (editedOneBalance) => AppData.Data.UserData.UpdateBalance(editedOneBalance));
            // Обработчик вызова обновления данных поступления.
            Connection.On<ReceiptDTO>("UpdateReceipt", (editedReceipt) => AppData.Data.UserData.UpdateReceipt(editedReceipt));
            // Обработчик вызова обновления данных списания.
            Connection.On<ExpenseDTO>("UpdateExpense", (editedExpense) => AppData.Data.UserData.UpdateExpense(editedExpense));
            // Обработчик вызова обновления данных поставщика.
            Connection.On<ProviderDTO>("UpdateProvider", (editedProvider) => AppData.Data.UserData.UpdateProvider(editedProvider));
            // Обработчик вызова обновления данных почты.
            Connection.On<EmailDTO>("UpdateEmail", (editedEmail) => AppData.Data.UserData.UpdateEmail(editedEmail));
            // Обработчик вызова обновления данных картриджа.
            Connection.On<CartridgeDTO>("UpdateCartridge", (editedCartridge) => AppData.Data.UserData.UpdateCartridge(editedCartridge));
            // Обработчик вызова обновления данных принтера.
            Connection.On<PrinterDTO>("UpdatePrinter", (editedPrinter) => AppData.Data.UserData.UpdatePrinter(editedPrinter));
            // Обработчик вызова обновления данных ОСП.
            Connection.On<OspDTO>("UpdateOsp", (editedOsp) => AppData.Data.UserData.UpdateOsp(editedOsp));
            // Обработчик вызова обновления данных пользователя.
            Connection.On<UserDTO>("UpdateUser", (editedUser) => AppData.Data.UserData.UpdateUser(editedUser));
        }

        /// <summary>
        /// Обработчик события восстановления подключения.
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        private Task Connection_Reconnecting(Exception arg)
        {
            Status = false;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Обработчик события переподключения к серверу.
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        private Task Connection_Reconnected(string arg)
        {
            Status = true;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Обработчик события разрыва соединения.
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        private Task Connection_Closed(Exception arg)
        {
            Status = false;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Подключение к серверу.
        /// </summary>
        public static ConnectionServer Connect { get; private set; }

        /// <summary>
        /// Создает подключение к серверу по строке подключения.
        /// </summary>
        /// <param name="connectionString">Строка подключения</param>
        public static void Create(string connectionString)
        {
            Connect = new ConnectionServer(connectionString);
        }
    }
}
