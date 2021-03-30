using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CartAccLibrary.Entities
{
    /// <summary>
    /// Совместимость принтеров и картриджей.
    /// </summary>
    [Table("Compatibility")]
    public class Compatibility
    {
        /// <summary>
        /// Id.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Совместимый картридж.
        /// </summary>
        [Required]
        public Cartridge Cartridge { get; set; }

        /// <summary>
        /// Совместимый принтер.
        /// </summary>
        [Required]
        public Printer Printer { get; set; }
    }
}
