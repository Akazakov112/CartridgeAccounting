using System;
using System.Windows;
using System.Windows.Input;
using CartAccClient.Model;
using CartAccClient.View;
using Microsoft.AspNetCore.SignalR.Client;

namespace CartAccClient.ViewModel
{
    /// <summary>
    /// ViewModel стартового окна.
    /// </summary>
    class StartAppVm : MainViewModel
    {
        private Visibility addBtnVisible;
        /// <summary>
        /// Свойство видиимости кнопки повтора подключения.
        /// </summary>
        public Visibility AddBtnVisible
        {
            get { return addBtnVisible; }
            set { addBtnVisible = value; RaisePropertyChanged(nameof(AddBtnVisible)); }
        }

        private bool status;
        /// <summary>
        /// Статус прогресс бара.
        /// </summary>
        public bool Status
        {
            get { return status; }
            set { status = value; RaisePropertyChanged(nameof(Status)); }
        }

        private string statusText;
        /// <summary>
        /// Текст статуса подключения.
        /// </summary>
        public string StatusText
        {
            get { return statusText; }
            set { statusText = value; RaisePropertyChanged(nameof(StatusText)); }
        }

        /// <summary>
        /// Команда открытия настроек.
        /// </summary>
        public ICommand Settings
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    // Открыть окно настроек.
                    new SettingsForm().ShowDialog();
                });
            }
        }

        /// <summary>
        /// Команда переподключения.
        /// </summary>
        public ICommand Reconnect
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    // Старт подключения.
                    StartConnectAsync();
                });
            }
        }

        /// <summary>
        /// Закрыть программу.
        /// </summary>
        public ICommand Exit
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    ServerConnect.Connection.StopAsync();
                    Application.Current.Shutdown();
                });
            }
        }

        /// <summary>
        /// Пустой конструктор.
        /// </summary>
        public StartAppVm()
        {
            // Старт подключения.
            StartConnectAsync();
        }


        /// <summary>
        /// Запускает подключение к серверу.
        /// </summary>
        private async void StartConnectAsync()
        {
            // Обработчик вызова запрета доступа.
            ServerConnect.Connection.On("AccessDenied", async () =>
            {
                StatusText = "Доступ запрещен.\nОбратитесь к своему руководителю.";
                await ServerConnect.Connection.StopAsync();
            });
            // Запустить анимацию прогресс бара.
            Status = true;
            // Вывести сообщение.
            StatusText = "Подключение к серверу...";
            // Скрыть дополнительные кнопки.
            AddBtnVisible = Visibility.Collapsed;
            try
            {
                // Запуск подключения к серверу.
                await ServerConnect.Connection.StartAsync();
            }
            catch (Exception ex)
            {
                // Вывести ошибку подключения и показать доп кнопки.
                StatusText = $"Не удалось установить подключение.\n{ex.Message}";
                AddBtnVisible = Visibility.Visible;
            }
            // Остановить прогресс бар.
            Status = false;
        }
    }
}
