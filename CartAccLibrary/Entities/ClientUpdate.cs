using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CartAccLibrary.Entities
{
    /// <summary>
    /// Обновление клиента.
    /// </summary>
    [Table("ClientUpdates")]
    public class ClientUpdate
    {
        /// <summary>
        /// Id.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Дата выпуска обновления.
        /// </summary>
        [Required]
        public DateTime Date { get; set; }

        /// <summary>
        /// Версия обновления.
        /// </summary>
        [Required]
        public int Version { get; set; }

        /// <summary>
        /// Информация об обновлении.
        /// </summary>
        [Required]
        [MaxLength(1000)]
        public string Info { get; set; }

        /// <summary>
        /// Имя файла обновления.
        /// </summary>
        [Required]
        [MaxLength(500)]
        public string Filename { get; set; }
    }
}
