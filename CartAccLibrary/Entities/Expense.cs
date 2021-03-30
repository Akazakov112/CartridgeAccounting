using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CartAccLibrary.Entities
{
    /// <summary>
    /// Расход.
    /// </summary>
    [Table("Expenses")]
    public class Expense
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
        /// Основание.
        /// </summary>
        [Required]
        [MaxLength(300)]
        public string Basis { get; set; }

        /// <summary>
        /// Автор расхода.
        /// </summary>
        [Required]
        public User User { get; set; }

        /// <summary>
        /// Взятый картридж.
        /// </summary>
        [Required]
        public Cartridge Cartridge { get; set; }

        /// <summary>
        /// Количество взятого картриджа.
        /// </summary>
        [Required]
        public int Count { get; set; }

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
