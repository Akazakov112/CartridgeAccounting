using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CartAccLibrary.Entities
{
    /// <summary>
    /// Поступление.
    /// </summary>
    [Table("Receipts")]
    public class Receipt
    {
        /// <summary>
        /// Id.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Номер.
        /// </summary>
        [Required]
        public int Number { get; set; }

        /// <summary>
        /// Дата.
        /// </summary>
        [Required]
        public DateTime Date { get; set; }

        /// <summary>
        /// Автор поступления.
        /// </summary>
        [Required]
        public User User { get; set; }

        /// <summary>
        /// Поставщик.
        /// </summary>
        public Provider Provider { get; set; }

        /// <summary>
        /// Список поступивших картриджей.
        /// </summary>
        [Required]
        public List<ReceiptCartridge> Cartridges { get; set; }

        /// <summary>
        /// Комментарий.
        /// </summary>
        [Required]
        [MaxLength(1000)]
        public string Comment { get; set; }

        /// <summary>
        /// Пометка об удалении.
        /// </summary>
        [Required]
        public bool Delete { get; set; }

        /// <summary>
        /// Пометка о редактировании.
        /// </summary>
        [Required]
        public bool Edit { get; set; }

        /// <summary>
        /// Принадлежность к ОСП.
        /// </summary>
        [Required]
        public Osp Osp { get; set; }
    }
}
