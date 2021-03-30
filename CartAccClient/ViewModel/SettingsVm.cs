using CartAccClient.Model;
using CartAccClient.View;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace CartAccClient.ViewModel
{
    /// <summary>
    /// ViewModel окна настроек.
    /// </summary>
    class SettingsVm : MainViewModel
    {
        /// <summary>
        /// Адрес сервера.
        /// </summary>
        public string ServerAddress { get; set; }

        /// <summary>
        /// Порт сервера.
        /// </summary>
        public string ServerPort { get; set; }

        /// <summary>
        /// Путь на сервере.
        /// </summary>
        public string ServerPath { get; set; }

        /// <summary>
        /// Использование темной темы.
        /// </summary>
        public bool UseDarkTheme { get; set; }

        /// <summary>
        /// Команда сохранения настроек.
        /// </summary>
        public ICommand Save
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    if (IsConnectChanged())
                    {
                        // Обновить измененные настройки.
                        AppConfiguration.ServerAddress = ServerAddress;
                        AppConfiguration.ServerPort = ServerPort;
                        AppConfiguration.ServerPath = ServerPath;
                        AppConfiguration.UseDarkTheme = UseDarkTheme;
                        // Сохранить настройки.
                        AppConfiguration.Save();
                        // Если подключение неактивно.
                        if (ServerConnect.Connection.State != Microsoft.AspNetCore.SignalR.Client.HubConnectionState.Connected)
                        {
                            // Перезапустить клиент.
                            Process.Start("CartAccClient.exe");
                            Process.GetCurrentProcess().Kill();
                        }
                        else
                        {
                            // Спросить у пользователя о действии при существующем подключении.
                            MessageBoxResult resultDialog = Alert.Show("Текущее подключение активно.\nСменить сервер и перезапустить?", "Настройки", MessageBoxButton.YesNo);
                            // В зависимости от результата диалога.
                            switch (resultDialog)
                            {
                                case MessageBoxResult.Yes:
                                    // Перезапустить клиент.
                                    Process.Start("CartAccClient.exe");
                                    Process.GetCurrentProcess().Kill();
                                    break;
                                case MessageBoxResult.No:
                                    break;
                            }
                        }
                    }
                    else
                    {
                        AppConfiguration.UseDarkTheme = UseDarkTheme;
                        // Сохранить настройки.
                        AppConfiguration.Save();
                    }
                    // Закрыть окно.
                    if (obj is Window window)
                    {
                        window.Close();
                    }
                },
                (canEx) => IsAnyChanged());
            }
        }

        /// <summary>
        /// Команда закрытия настроек без сохранения.
        /// </summary>
        public ICommand Close
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    // Прочитать данные из файла конфигурации.
                    AppConfiguration.Read();
                    if (obj is Window window)
                    {
                        window.Close();
                    }
                });
            }
        }

        /// <summary>
        /// Конструктор.
        /// </summary>
        public SettingsVm()
        {
            ServerAddress = AppConfiguration.ServerAddress;
            ServerPort = AppConfiguration.ServerPort;
            ServerPath = AppConfiguration.ServerPath;
            UseDarkTheme = AppConfiguration.UseDarkTheme;
        }


        /// <summary>
        /// Проверка на изменение настроек.
        /// </summary>
        /// <returns>Были ли изменены настройки</returns>
        private bool IsAnyChanged()
        {
            return AppConfiguration.ServerAddress != ServerAddress
                || AppConfiguration.ServerPort != ServerPort
                || AppConfiguration.ServerPath != ServerPath
                || AppConfiguration.UseDarkTheme != UseDarkTheme;
        }

        /// <summary>
        /// Проверка на изменение настроек подключения.
        /// </summary>
        /// <returns>Были ли изменены настройки подключения</returns>
        private bool IsConnectChanged()
        {
            return AppConfiguration.ServerAddress != ServerAddress
                || AppConfiguration.ServerPort != ServerPort
                || AppConfiguration.ServerPath != ServerPath;
        }
    }
}
