using CartAccLibrary.Entities;
using Microsoft.EntityFrameworkCore;

namespace CartAccServer.Models.Infrastructure
{
    /// <summary>
    /// Контекст базы данных учета картриджей.
    /// </summary>
    public class CartAccDbContext : DbContext
    {
        /// <summary>
        /// Все уровни доступа.
        /// </summary>
        public DbSet<Access> Accesses { get; set; }

        /// <summary>
        /// Все картриджи.
        /// </summary>
        public DbSet<Cartridge> Cartridges { get; set; }

        /// <summary>
        /// Все принтеры.
        /// </summary>
        public DbSet<Printer> Printers { get; set; }

        /// <summary>
        /// Все ОСП.
        /// </summary>
        public DbSet<Osp> Osps { get; set; }

        /// <summary>
        /// Обновления клиента.
        /// </summary>
        public DbSet<ClientUpdate> Updates { get; set; }


        /// <summary>
        /// Балансы картриджей.
        /// </summary>
        public DbSet<Balance> Balances { get; set; }

        /// <summary>
        /// Адреса электронной почты.
        /// </summary>
        public DbSet<Email> Emails { get; set; }

        /// <summary>
        /// Списания.
        /// </summary>
        public DbSet<Expense> Expenses { get; set; }

        /// <summary>
        /// Поставщики.
        /// </summary>
        public DbSet<Provider> Providers { get; set; }

        /// <summary>
        /// Поступления.
        /// </summary>
        public DbSet<Receipt> Receipts { get; set; }

        /// <summary>
        /// Картриджи поступлений.
        /// </summary>
        public DbSet<ReceiptCartridge> ReceiptCartridges { get; set; }

        /// <summary>
        /// Пользователи.
        /// </summary>
        public DbSet<User> Users { get; set; }


        /// <summary>
        /// Конструктор с настройкой.
        /// </summary>
        /// <param name="options"></param>
        public CartAccDbContext(DbContextOptions<CartAccDbContext> options) : base(options)
        {
            ChangeTracker.LazyLoadingEnabled = false;
        }
    }
}
