using CartAccLibrary.Entities;
using Microsoft.EntityFrameworkCore;

namespace CartAccNotifier.Models
{
    /// <summary>
    /// Контекст базы данных учета картриджей.
    /// </summary>
    class CartAccDbContext : DbContext
    {
        /// <summary>
        /// Все ОСП.
        /// </summary>
        public DbSet<Osp> Osps { get; set; }

        /// <summary>
        /// Балансы картриджей.
        /// </summary>
        public DbSet<Balance> Balances { get; set; }

        /// <summary>
        /// Адреса электронной почты.
        /// </summary>
        public DbSet<Email> Emails { get; set; }

        /// <summary>
        /// Конструктор с настройкой.
        /// </summary>
        /// <param name="options"></param>
        public CartAccDbContext(DbContextOptions<CartAccDbContext> options) : base(options)
        {
            this.ChangeTracker.LazyLoadingEnabled = false;
        }
    }
}
