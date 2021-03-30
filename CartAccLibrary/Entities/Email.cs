using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CartAccLibrary.Entities
{
    /// <summary>
    /// Электронная почта для ОСП.
    /// </summary>
    [Table("Emails")]
    public class Email
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
        /// Адрес.
        /// </summary>
        [Required]
        [MaxLength(150)]
        public string Address { get; set; }

        /// <summary>
        /// Статус активности.
        /// </summary>
        [Required]
        public bool Active { get; set; }

        /// <summary>
        /// Принадлежность к ОСП.
        /// </summary>
        [Required]
        public Osp Osp { get; set; }
    }
}
