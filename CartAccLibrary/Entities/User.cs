using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CartAccLibrary.Entities
{
    /// <summary>
    /// Пользователь.
    /// </summary>
    [Table("Users")]
    public class User
    {
        /// <summary>
        /// Id.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Логин пользователя.
        /// </summary>
        [Required]
        [MaxLength(150)]
        public string Login { get; set; }

        /// <summary>
        /// Полное имя.
        /// </summary>
        [Required]
        [MaxLength(300)]
        public string Fullname { get; set; }

        /// <summary>
        /// Уровень доступа.
        /// </summary>
        [Required]
        public Access Access { get; set; }

        /// <summary>
        /// Статус активности.
        /// </summary>
        [Required]
        public bool Active { get; set; }

        /// <summary>
        /// ОСП, к которому относится пользователь.
        /// </summary>
        [Required]
        public Osp Osp { get; set; }
    }
}
