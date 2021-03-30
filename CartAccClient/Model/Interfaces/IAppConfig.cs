namespace CartAccClient.Model.Interfaces
{
    /// <summary>
    /// Интерфейс настроек программы.
    /// </summary>
    interface IAppConfig
    {
        /// <summary>
        /// Адрес сервера.
        /// </summary>
        string ServerAddress { get; set; }

        /// <summary>
        /// Порт сервера.
        /// </summary>
        string ServerPort { get; set; }

        /// <summary>
        /// Путь на сервере.
        /// </summary>
        string ServerPath { get; set; }

        /// <summary>
        /// Использование темной темы.
        /// </summary>
        bool UseDarkTheme { get; set; }

        /// <summary>
        /// Сохранить настройки.
        /// </summary>
        void Save();

        /// <summary>
        /// Прочесть настройки.
        /// </summary>
        void Read();

        /// <summary>
        /// Установить тему оформления.
        /// </summary>
        void SetTheme();

        /// <summary>
        /// Получить строку подключения.
        /// </summary>
        /// <returns></returns>
        string GetConnectionString();
    }
}
