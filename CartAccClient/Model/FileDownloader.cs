using System;
using System.Net;
using System.Threading.Tasks;

namespace CartAccClient.Model
{
    /// <summary>
    /// Скачивает файл по указанному ресурсу.
    /// </summary>
    class FileDownloader
    {
        /// <summary>
        /// Ссылка  на скачивание.
        /// </summary>
        private readonly Uri downloadUrl;

        /// <summary>
        /// Конструктор с ссылкой на скачивание.
        /// </summary>
        /// <param name="downloadUrl">Ссылка на скачивание</param>
        public FileDownloader(Uri downloadUrl)
        {
            this.downloadUrl = downloadUrl;
        }


        /// <summary>
        /// Скачивает файл.
        /// </summary>
        /// <returns>Результат выполнения</returns>
        public async Task<bool> DownloadFileAsync()
        {
            try
            {
                // Скачать файл по ссылке.
                using (WebClient client = new WebClient())
                {
                    client.UseDefaultCredentials = true;
                    string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                    await Task.Run(() => client.DownloadFile(downloadUrl, $@"{appDataPath}\Update.zip"));
                    return true;
                };
            }
            catch
            {
                return false;
            }
        }
    }
}
