using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace Updater.Model
{
    /// <summary>
    /// Обновляет клиента.
    /// </summary>
    class AppUpdater : BaseVm
    {
        /// <summary>
        /// Путь к файлу обновления.
        /// </summary>
        private readonly string updateFilePath;

        /// <summary>
        /// Путь к каталогу программы.
        /// </summary>
        private readonly string appPath;

        /// <summary>
        /// Инициализирует обновление клиента.
        /// </summary>
        /// <param name="updateFilepath">Путь к файлу обновления</param>
        /// <param name="appCatalog">Каталог с программой</param>
        public AppUpdater(string updateFilepath, string appCatalog)
        {
            updateFilePath = updateFilepath;
            appPath = appCatalog;
        }

        /// <summary>
        /// Устанавливает обновление. Возвращает результат.
        /// </summary>
        /// <returns></returns>
        public async Task<bool> InstallAsync()
        {
            // Распаковать архив с перезаписыванием файлов.
            return await Task.Run(() =>
            {
                // Если файл обновления существует.
                if (File.Exists(updateFilePath))
                {
                    try
                    {
                        // Удалить каталог библиотек с вложенными файлами.
                        Directory.Delete($@"{appPath}\Libs", true);
                        // Удалить файлы приложения.
                        foreach (var t in Directory.GetFiles(appPath))
                        {
                            File.Delete(t);
                        }
                        // Распаковать архив с обновлениями.
                        ZipFile.ExtractToDirectory(updateFilePath, appPath);
                        // Удалить файл обновления.
                        File.Delete(updateFilePath);
                        // Если есть файл запуска клиента.
                        if (File.Exists(appPath + @"\CartAccClient.exe"))
                        {
                            // Запустить клиент учета.
                            Process.Start(appPath + @"\CartAccClient.exe");
                            // Закрыть утилиту обновления.
                            Process.GetCurrentProcess().Kill();
                        }
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }
                // Если файла обновления нет вернуть ложь.
                else
                {
                    return false;
                }
            });
        }
    }
}
