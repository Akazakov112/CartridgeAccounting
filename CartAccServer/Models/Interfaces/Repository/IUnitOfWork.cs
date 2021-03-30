using System;
using CartAccLibrary.Entities;

namespace CartAccServer.Models.Interfaces.Repository
{
    /// <summary>
    /// Интерфейс объекта работы с репозиториями.
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// Репозиторий уровней доступа.
        /// </summary>
        IRepository<Access> Accesses { get; }

        /// <summary>
        /// Репозиторий обновлений клиента.
        /// </summary>
        IRepository<ClientUpdate> Updates { get; }

        /// <summary>
        /// Репозиторий картриджей.
        /// </summary>
        IRepository<Cartridge> Cartridges { get; }

        /// <summary>
        /// Репозиторий принтеров.
        /// </summary>
        IRepository<Printer> Printers { get; }

        /// <summary>
        /// Репозиторий ОСП.
        /// </summary>
        IRepository<Osp> Osps { get; }

        /// <summary>
        /// Репозиторий баланса.
        /// </summary>
        IRepository<Balance> Balances { get; }

        /// <summary>
        /// Репозиторий электронной почты.
        /// </summary>
        IRepository<Email> Emails { get; }

        /// <summary>
        /// Репозиторий списаний.
        /// </summary>
        IRepository<Expense> Expenses { get; }

        /// <summary>
        /// Репозиторий поставщиков.
        /// </summary>
        IRepository<Provider> Providers { get; }

        /// <summary>
        /// Репозиторий поступлений.
        /// </summary>
        IRepository<Receipt> Receipts { get; }

        /// <summary>
        /// Картриджи постулпений.
        /// </summary>
        IRepository<ReceiptCartridge> ReceiptCartridges { get; }

        /// <summary>
        /// Репозиторий пользователей.
        /// </summary>
        IRepository<User> Users { get; }

        /// <summary>
        /// Сохранить изменения в БД.
        /// </summary>
        void Save();
    }
}
