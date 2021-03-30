using System;
using Updater.Model;

namespace Updater.ViewModel
{
    /// <summary>
    /// ViewModel главного окна.
    /// </summary>
    class MainFormVm : BaseVm
    {
        private bool isInstalling;
        private string statusText;

        private readonly string appPath;

        /// <summary>
        /// Объект установки обновления.
        /// </summary>
        public AppUpdater Updater { get; set; }

        /// <summary>
        /// Статус установки.
        /// </summary>
        public bool IsInstalling
        {
            get { return isInstalling; }
            set { isInstalling = value; RaisePropertyChanged(nameof(IsInstalling)); }
        }

        /// <summary>
        /// Текст статуса в прогрессбаре.
        /// </summary>
        public string StatusText
        {
            get { return statusText; }
            set { statusText = value; RaisePropertyChanged(nameof(StatusText)); }
        }

        /// <summary>
        /// Конструктор без параметров.
        /// </summary>
        public MainFormVm()
        {
            // Получить расположение программы.
            appPath = Environment.CurrentDirectory.Replace(@"Updater", "");
            // Получить расположение файла обновления.
            string updateFilePath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\Update.zip";
            // Инициализировать объект установки.
            Updater = new AppUpdater(updateFilePath, appPath);
            // Запустить установку асинхронно.
            StartUpdateAsync();
        }

        /// <summary>
        /// Запускает обновление.
        /// </summary>
        private async void StartUpdateAsync()
        {
            // Статус установки.
            StatusText = "Установка...";
            IsInstalling = true;
            // Вывести информацию в зависиимости от результата установки.
            StatusText = await Updater.InstallAsync() ? "Установка завершена" : "Ошибка установки";
            // Статус установки.
            IsInstalling = false;
        }
    }
}
