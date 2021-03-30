using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using CartAccClient.Model;
using CartAccLibrary.Dto;

namespace CartAccClient.ViewModel
{
    /// <summary>
    /// ViewModel окна обновления клиента.
    /// </summary>
    class UpdateAppVm : MainViewModel
    {
        private bool isEnableBtn;
        private bool statusUpdate;
        private string statusText;
        private readonly string updaterPath;

        /// <summary>
        /// Данные обновления.
        /// </summary>
        public ClientUpdateDTO Update { get; }

        /// <summary>
        /// Статус процесса обновления.
        /// </summary>
        public bool StatusUpdate
        {
            get { return statusUpdate; }
            set { statusUpdate = value; RaisePropertyChanged(nameof(StatusUpdate)); }
        }

        /// <summary>
        /// Текст статуса процесса обновления.
        /// </summary>
        public string StatusText
        {
            get { return statusText; }
            set { statusText = value; RaisePropertyChanged(nameof(StatusText)); }
        }

        /// <summary>
        /// Запустить обновление.
        /// </summary>
        public ICommand StartUpdate
        {
            get
            {
                return new RelayCommand(async obj =>
                {
                    // Вывести инфо.
                    isEnableBtn = false;
                    StatusUpdate = true;
                    StatusText = "Загрузка обновления";
                    // Если обновление скачано.
                    if (await new FileDownloader(Update.DownloadLink).DownloadFileAsync())
                    {
                        // Вывести инфо.
                        isEnableBtn = true;
                        StatusUpdate = false;
                        StatusText = "Загрузка завершена";
                        // Если файлы утилиты обновления существуют.
                        if (File.Exists(updaterPath))
                        {
                            // Запустить утилиту обновления.
                            Process.Start(updaterPath);
                            // Закрыть программу.
                            Process.GetCurrentProcess().Kill();
                        }
                        // Иначе вывести ошибку.
                        else
                        {
                            StatusUpdate = false;
                            StatusText = "Ошибка установки обновления";
                        }
                    }
                    // Иначе вывести ошибку.
                    else
                    {
                        StatusUpdate = false;
                        StatusText = "Ошибка загрузки";
                    }
                }, 
                // Команда доступна, когда нет загрузки.
                (canEx) => isEnableBtn);
            }
        }

        /// <summary>
        /// Закрыть окно.
        /// </summary>
        public ICommand Close
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    if (obj is Window window)
                    {
                        // Закрыть окно.
                        window.Close();
                    }
                },
                // Команда доступна, когда нет загрузки.
                (canEx) => isEnableBtn);
            }
        }

        /// <summary>
        /// Пустой конструктор.
        /// </summary>
        public UpdateAppVm() { }

        /// <summary>
        /// Конструктор с данными обновления.
        /// </summary>
        /// <param name="update">Данные обновления</param>
        /// <param name="downloadUrl">Ссылка на скачивание</param>
        public UpdateAppVm(ClientUpdateDTO update)
        {
            isEnableBtn = true;
            Update = update;
            StatusText = "*Обновление можно запустить позже из меню \"Справка\" главного окна.";
            StatusUpdate = false;
            updaterPath = Environment.CurrentDirectory + @"\Updater\Updater.exe";
        }
    }
}
