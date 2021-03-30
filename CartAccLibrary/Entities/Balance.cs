using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CartAccLibrary.Services;

namespace CartAccLibrary.Entities
{
    /// <summary>
    /// Картриджи для составления баланса.
    /// </summary>
    [Table("Balances")]
    public class Balance : BaseVm
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

        /// <summary>
        /// Статус использования.
        /// </summary>
        [Required]
        public bool InUse { get; set; }

        /// <summary>
        /// Принадлежность к ОСП.
        /// </summary>
        [Required]
        public Osp Osp { get; set; }
    }
}
