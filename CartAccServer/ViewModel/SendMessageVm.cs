using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CartAccServer.Models.Infrastructure;

namespace CartAccServer.ViewModel
{
    /// <summary>
    /// ViewModel представления отправки сообщений.
    /// </summary>
    public class SendMessageVm
    {
        /// <summary>
        /// Список получателей.
        /// </summary>
        public List<Recepient> Recipients { get; set; }

        /// <summary>
        /// Выбранный получатель.
        /// </summary>
        [Required(ErrorMessage = "Не выбран получатель.")]
        public string RecepientConnectionId { get; set; }

        /// <summary>
        /// Текст сообщения.
        /// </summary>
        [Required(ErrorMessage = "Введите сообщение.")]
        [StringLength(500, MinimumLength = 3, ErrorMessage = "Длина строки должна быть от 3 до 500 символов")]
        public string Message { get; set; }
    }
}
