using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CartAccLibrary.Entities
{
    /// <summary>
    /// Картридж.
    /// </summary>
    [Table("Cartridges")]
    public class Cartridge
    {
        /// <summary>
        /// Id.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Модель.
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Model { get; set; }

        /// <summary>
        /// Совместимые принтеры.
        /// </summary>
        public List<Compatibility> Compatibility { get; set; }

        /// <summary>
        /// Статус использования.
        /// </summary>
        [Required]
        public bool InUse { get; set; }
    }
}