using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CartAccLibrary.Entities
{
    /// <summary>
    /// Поступивший картридж.
    /// </summary>
    [Table("ReceiptCarts")]
    public class ReceiptCartridge
    {
        /// <summary>
        /// Id.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Картридж.
        /// </summary>
        [Required]
        public Cartridge Cartridge { get; set; }

        /// <summary>
        /// Количество.
        /// </summary>
        [Required]
        public int Count { get; set; }
    }
}
