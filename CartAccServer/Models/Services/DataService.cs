using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CartAccLibrary.Dto;
using CartAccLibrary.Entities;
using CartAccServer.Models.Interfaces.Repository;
using CartAccServer.Models.Interfaces.Services;

namespace CartAccServer.Models.Services
{
    /// <summary>
    /// Сервис работы со всеми данными.
    /// </summary>
    public class DataService : IDataService
    {
        public IUnitOfWork Database { get; }

        public IEntityService<Access, AccessDTO> Accesses { get; }

        public IEntityService<Cartridge, CartridgeDTO> Cartridges { get; }

        public IEntityService<Printer, PrinterDTO> Printers { get; }

        public IEntityService<Osp, OspDTO> Osps { get; }

        public IEntityService<User, UserDTO> Users { get; }

        public IEntityService<Balance, BalanceDTO> Balance { get; }

        public IEntityService<Email, EmailDTO> Emails { get; }

        public IEntityService<Expense, ExpenseDTO> Expenses { get; }

        public IEntityService<Provider, ProviderDTO> Providers { get; }

        public IEntityService<Receipt, ReceiptDTO> Receipts { get; }

        public DataService(IUnitOfWork unitOfWork)
        {
            Database = unitOfWork;
            Accesses = new AccessService(Database);
            Cartridges = new CartridgeService(Database);
            Printers = new PrinterService(Database);
            Osps = new OspService(Database);
            Balance = new BalanceService(Database);
            Emails = new EmailService(Database);
            Expenses = new ExpenseService(Database);
            Providers = new ProviderService(Database);
            Receipts = new ReceiptService(Database);
            Users = new UserService(Database);
        }


        public OspDataDTO GetOspData(int userId, int ospId)
        {
            // Период получения документов в ОСА (30 дней).
            DateTime startPeriod = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day).AddDays(-30);
            DateTime endPeriod = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            // Получить общие списки.
            ICollection<AccessDTO> allAccesses = Accesses.GetAll();
            ICollection<UserDTO> allUsers = Users.GetAll();
            ICollection<CartridgeDTO> allCarts = Cartridges.GetAll();
            ICollection<PrinterDTO> allPrints = Printers.GetAll();
            ICollection<OspDTO> allOsps = Osps.GetAll();
            // Получить данные по ОСП.
            ICollection<BalanceDTO> balances = Balance.Find(x => x.Osp.Id == ospId);
            ICollection<EmailDTO> emails = Emails.Find(x => x.Osp.Id == ospId);
            ICollection<ProviderDTO> providers = Providers.Find(x => x.Osp.Id == ospId);
            ICollection<ReceiptDTO> receipts = Receipts.Find(u => u.Osp.Id == ospId && u.Date >= startPeriod && u.Date <= endPeriod);
            ICollection<ExpenseDTO> expenses = Expenses.Find(u => u.Osp.Id == ospId && u.Date >= startPeriod && u.Date <= endPeriod);
            ICollection<ExpenseDTO> userExpenses = Expenses.Find(u => u.User.Id == userId && u.Osp.Id == ospId && u.Date >= startPeriod && u.Date <= endPeriod);
            // Создать объект данных для пользователя.
            var userDataDTO = new OspDataDTO()
            {
                Accesses = new ObservableCollection<AccessDTO>(allAccesses),
                Osps = new ObservableCollection<OspDTO>(allOsps),
                Cartridges = new ObservableCollection<CartridgeDTO>(allCarts),
                Printers = new ObservableCollection<PrinterDTO>(allPrints),
                Balance = new ObservableCollection<BalanceDTO>(balances),
                Expenses = new ObservableCollection<ExpenseDTO>(expenses),
                UserExpenses = new ObservableCollection<ExpenseDTO>(userExpenses),
                Providers = new ObservableCollection<ProviderDTO>(providers),
                Receipts = new ObservableCollection<ReceiptDTO>(receipts),
                Emails = new ObservableCollection<EmailDTO>(emails),
                Users = new ObservableCollection<UserDTO>(allUsers)
            };
            return userDataDTO;
        }
    }
}
