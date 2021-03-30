using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CartAccLibrary.Entities;
using Microsoft.AspNetCore.Http;

namespace CartAccServer.ViewModel
{
    /// <summary>
    /// ViewModel представления загрузки обновлений клиента.
    /// </summary>
    public class UploadUpdateVm
    {
        /// <summary>
        /// Все загруженные обновления.
        /// </summary>
        public IEnumerable<ClientUpdate> AllUpdates { get; set; }

        /// <summary>
        /// Файл загружаемого обновления.
        /// </summary>
        [Required(ErrorMessage = "Не выбран файл.")]
        public IFormFile UpdateFile { get; set; }

        /// <summary>
        /// Описание обновления.
        /// </summary>
        [Required(ErrorMessage = "Не указано описание обновления.")]
        public string Description { get; set; }

        /// <summary>
        /// Версия обновления.
        /// </summary>
        [Required(ErrorMessage = "Не указана версия обновления.")]
        [Range(1000, 9999, ErrorMessage = "Номер версии должен быть 4-х значный.")]
        public int Version { get; set; }
    }
}
