using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CartAccLibrary.Entities
{
    /// <summary>
    /// Поставщик картриджей.
    /// </summary>
    [Table("Providers")]
    public class Provider
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
        /// Название.
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Name { get; set; }

        /// <summary>
        /// Адрес электронной почты.
        /// </summary>
        [MaxLength(150)]
        public string Email { get; set; }

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
