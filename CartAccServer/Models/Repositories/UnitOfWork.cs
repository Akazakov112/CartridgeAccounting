using CartAccLibrary.Entities;
using CartAccServer.Models.Infrastructure;
using CartAccServer.Models.Interfaces.Repository;
using System;

namespace CartAccServer.Models.Repositories
{
    /// <summary>
    /// Работа с репозиториями.
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        /// <summary>
        /// Состояние контекста БД.
        /// </summary>
        private bool disposed;

        /// <summary>
        /// Контекст БД.
        /// </summary>
        private readonly CartAccDbContext dbContext;

        /// <summary>
        /// Репозиторий уровней доступа.
        /// </summary>
        public IRepository<Access> Accesses { get; }

        /// <summary>
        /// Репозиторий обновлений клиента.
        /// </summary>
        public IRepository<ClientUpdate> Updates { get; }

        /// <summary>
        /// Репозиторий картриджей.
        /// </summary>
        public IRepository<Cartridge> Cartridges { get; }

        /// <summary>
        /// Репозиторий принтеров.
        /// </summary>
        public IRepository<Printer> Printers { get; }

        /// <summary>
        /// Репозиторий ОСП.
        /// </summary>
        public IRepository<Osp> Osps { get; }

        /// <summary>
        /// Репозиторий баланса.
        /// </summary>
        public IRepository<Balance> Balances { get; }

        /// <summary>
        /// Репозиторий электронной почты ОСП.
        /// </summary>
        public IRepository<Email> Emails { get; }

        /// <summary>
        /// Репозиторий списаний.
        /// </summary>
        public IRepository<Expense> Expenses { get; }

        /// <summary>
        /// Репозиторий поставщиков.
        /// </summary>
        public IRepository<Provider> Providers { get; }

        /// <summary>
        /// Репозиторий поступлений.
        /// </summary>
        public IRepository<Receipt> Receipts { get; }

        /// <summary>
        /// Репозиторий пользователей.
        /// </summary>
        public IRepository<User> Users { get; }

        /// <summary>
        /// Картриджи постулпений.
        /// </summary>
        public IRepository<ReceiptCartridge> ReceiptCartridges { get; }

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="context">Контекст БД</param>
        public UnitOfWork(CartAccDbContext context)
        {
            dbContext = context;
            Accesses = new AccessRepository(dbContext);
            Users = new UserRepository(dbContext);
            Osps = new OspRepository(dbContext);
            Updates = new ClientUpdateRepository(dbContext);
            Cartridges = new CartridgeRepository(dbContext);
            Printers = new PrinterRepository(dbContext);
            Balances = new BalanceRepository(dbContext);
            Emails = new EmailRepository(dbContext);
            Expenses = new ExpenseRepository(dbContext);
            Providers = new ProviderRepository(dbContext);
            Receipts = new ReceiptRepository(dbContext);
            ReceiptCartridges = new ReceiptCartsRepository(dbContext);
        }


        /// <summary>
        /// Dispose контекста БД.
        /// </summary>
        /// <param name="disposing"></param>
        public virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    dbContext.Dispose();
                }
                disposed = true;
            }
        }

        /// <summary>
        /// Освободить объект из памяти.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Сохранить изменения в БД.
        /// </summary>
        public void Save()
        {
            dbContext.SaveChanges();
        }
    }
}
