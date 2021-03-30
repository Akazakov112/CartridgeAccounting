using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CartAccLibrary.Entities
{
    /// <summary>
    /// ОСП.
    /// </summary>
    [Table("Osps")]
    public class Osp
    {
        /// <summary>
        /// Id.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Название.
        /// </summary>
        [Required]
        [MaxLength(300)]
        public string Name { get; set; }

        /// <summary>
        /// Статус подключения к учету.
        /// </summary>
        [Required]
        public bool Active { get; set; }

        /// <summary>
        /// Баланс картриджей.
        /// </summary>
        [Required]
        public List<Balance> Balances { get; set; }

        /// <summary>
        /// Список расходов.
        /// </summary>
        [Required]
        public List<Expense> Expenses { get; set; }

        /// <summary>
        /// Список поступлений.
        /// </summary>
        [Required]
        public List<Receipt> Receipts { get; set; }

        /// <summary>
        /// Список поставщиков.
        /// </summary>
        [Required]
        public List<Provider> Providers { get; set; }

        /// <summary>
        /// Список электронных адресов.
        /// </summary>
        [Required]
        public List<Email> Emails { get; set; }

        /// <summary>
        /// Список пользователей в ОСП.
        /// </summary>
        [Required]
        public List<User> Users { get; set; }
    }
}
