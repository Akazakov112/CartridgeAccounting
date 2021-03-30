using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CartAccLibrary.Dto;
using CartAccServer.Models.Interfaces.Services;

namespace CartAccServer.Models.Services
{
    /// <summary>
    /// Проводит инвентаризацию по балансу.
    /// </summary>
    public class InventoryOfBalanceService : IInventoryService
    {
        /// <summary>
        /// Сервис работы с данными.
        /// </summary>
        private IDataService DataService { get; }

        /// <summary>
        /// ОСП проведения инвентаризации.
        /// </summary>
        private OspDTO Osp { get; }

        /// <summary>
        /// Пользователь, автор инвентаризации.
        /// </summary>
        private UserDTO User { get; }

        /// <summary>
        /// Баланс ОСП для проведения инвентаризации.
        /// </summary>
        private IEnumerable<InventBalanceDTO> EditedBalance { get; }

        /// <summary>
        /// Списания по результатам инвентаризации.
        /// </summary>
        public List<ExpenseDTO> Expenses { get; }

        /// <summary>
        /// Поступление по результатам инвентаризации.
        /// </summary>
        public ReceiptDTO Receipt { get; private set; }

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="ospid">Id ОСП</param>
        /// <param name="userId">Id пользователя</param>
        /// <param name="editedBalance">Редактированный баланс</param>
        /// <param name="dataService">Объект работы с БД</param>
        public InventoryOfBalanceService(int ospId, int userId, IEnumerable<InventBalanceDTO> editedBalance, IDataService dataService)
        {
            DataService = dataService;
            Osp = DataService.Osps.Get(ospId);
            User = DataService.Users.Get(userId);
            EditedBalance = editedBalance;
            Expenses = new List<ExpenseDTO>();
            MakeInventory();
        }


        /// <summary>
        /// Проводит инвентаризацию. Создает документы по результатам.
        /// </summary>
        private void MakeInventory()
        {
            // Создать список картриджей для коррекционного поступления.
            ObservableCollection<ReceiptCartridgeDTO> receiptCarts = new ObservableCollection<ReceiptCartridgeDTO>();
            // Перебрать баланс для инвентаризации.
            foreach (InventBalanceDTO newBalance in EditedBalance)
            {
                // Значение фактического остатка.
                int factCount = (int)newBalance.FactCount;
                // Найти редактируемый баланс в бд.
                BalanceDTO currentBalance = DataService.Balance.Get(newBalance.Id);
                // Если фактический остаток больше текущего.
                if (factCount > currentBalance.Count)
                {
                    // Создать картридж поступления.
                    var receipCart = new ReceiptCartridgeDTO()
                    {
                        Cartridge = new CartridgeDTO(currentBalance.Cartridge.Id, currentBalance.Cartridge.Model, new ObservableCollection<PrinterDTO>(), currentBalance.Cartridge.InUse),
                        Count = factCount - currentBalance.Count
                    };
                    // Добавить картридж в коллекцию картриджей поступления.
                    receiptCarts.Add(receipCart);
                }
                // Если фактический остаток меньше текущего.
                else if (factCount < currentBalance.Count)
                {
                    // Найти последнее списание в БД.
                    ExpenseDTO lastExpense = DataService.Expenses.Find(x => x.Osp.Id == Osp.Id).LastOrDefault();
                    // Создать корректирующее списание.
                    var expense = new ExpenseDTO()
                    {
                        Basis = "Коррекция по результатам инвентаризации",
                        Cartridge = currentBalance.Cartridge,
                        Date = DateTime.Now.Date,
                        Count = currentBalance.Count - factCount,
                        Delete = false,
                        Edit = false,
                        Number = lastExpense is null ? 1 : lastExpense.Number + 1,
                        User = User,
                        OspId = Osp.Id
                    };
                    // Добавить корректирующее списание в БД.
                    DataService.Expenses.Add(expense);
                    // Получить добавленное списание с присвоенным Id.
                    ExpenseDTO addedExpense = DataService.Expenses.Find(x => x.Osp.Id == Osp.Id).LastOrDefault();
                    // Добавить его в список созданных по результатам инвентаризации списаний.
                    Expenses.Add(addedExpense);
                }
            }
            // Если были добавлены картриджи для корректирующего поступления.
            if (receiptCarts.Any())
            {
                // Найти последнее поступление в БД.
                ReceiptDTO lastReceipt = DataService.Receipts.Find(x => x.Osp.Id == Osp.Id).LastOrDefault();
                // Создать корректирующее поступление.
                var receipt = new ReceiptDTO()
                {
                    Date = DateTime.Now.Date,
                    Comment = "Коррекция по результатам инвентаризации",
                    Delete = false,
                    Edit = false,
                    Number = lastReceipt is null ? 1 : lastReceipt.Number + 1,
                    Cartridges = receiptCarts,
                    Provider = null,
                    User = User,
                    OspId = Osp.Id,
                };
                // Добавить корректирующее поступление в БД.
                DataService.Receipts.Add(receipt);
                // Получить добавленное поступление с присвоенным Id и присвоить его свойству.
                Receipt = DataService.Receipts.Find(x => x.Osp.Id == Osp.Id).LastOrDefault();
            }
        }
    }
}
