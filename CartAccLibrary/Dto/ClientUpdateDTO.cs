using System;

namespace CartAccLibrary.Dto
{
    /// <summary>
    /// DTO класс обновления клиента.
    /// </summary>
    public class ClientUpdateDTO
    {
        /// <summary>
        /// Дата выпуска обновления.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Версия обновления.
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// Информация об обновлении.
        /// </summary>
        public string Info { get; set; }

        /// <summary>
        /// Имя файла обновления.
        /// </summary>
        public string Filename { get; set; }

        /// <summary>
        /// Ссылка на скачивание обновления.
        /// </summary>
        public Uri DownloadLink { get; set; }

        /// <summary>
        /// Пустой конструктор.
        /// </summary>
        public ClientUpdateDTO() { }

        /// <summary>
        /// Полный конструктор.
        /// </summary>
        /// <param name="date">Дата обновления</param>
        /// <param name="version">Версия клиента</param>
        /// <param name="info">Информация об обновлении</param>
        public ClientUpdateDTO(DateTime date, int version, string info, string filename, Uri downloadLink)
        {
            Date = date;
            Version = version;
            Info = info;
            Filename = filename;
            DownloadLink = downloadLink;
        }
    }
}
