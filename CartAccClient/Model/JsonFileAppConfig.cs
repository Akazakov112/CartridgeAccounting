using System;
using System.IO;
using System.Reflection;
using System.Windows;
using CartAccClient.Model.Interfaces;
using CartAccClient.View;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CartAccClient.Model
{
    /// <summary>
    /// Класс настроек приложения.
    /// </summary>
    class JsonFileAppConfig : IAppConfig
    {
        /// <summary>
        /// Путь к папке программы.
        /// </summary>
        private readonly static string appPath;

        /// <summary>
        /// Путь к папке настроек.
        /// </summary>
        private readonly static string configPath;

        /// <summary>
        /// Путь к файлу настроек.
        /// </summary>
        private readonly static string configFilePath;

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

        private bool useDarkTheme;
        /// <summary>
        /// Использование темной темы.
        /// </summary>
        public bool UseDarkTheme
        {
            get { return useDarkTheme; }
            set
            {
                useDarkTheme = value;
                SetTheme();
            }
        }

        /// <summary>
        /// Конструктор.
        /// </summary>
        private JsonFileAppConfig()
        {
            ServerAddress = "P-OKS-SQL-1";
            ServerPort = "5050";
            ServerPath = "cartaccuser";
            UseDarkTheme = true;
        }

        /// <summary>
        /// Конструктор типа.
        /// </summary>
        static JsonFileAppConfig()
        {
            appPath = Environment.CurrentDirectory;
            configPath = Path.Combine(appPath, @"Config\");
            configFilePath = Path.Combine(appPath, @"Config\Configuration.json");
        }


        /// <summary>
        /// Сохраняет настройки в файл.
        /// </summary>
        public void Save()
        {
            // Сериализовать и записать файл настроек.
            string configuration = JsonConvert.SerializeObject(this);
            try
            {
                // Если каталог настроек отсутствует, создать его.
                if (!Directory.Exists(configPath))
                {
                    Directory.CreateDirectory(configPath);
                }
                File.WriteAllText(configFilePath, configuration);
            }
            catch (Exception ex)
            {
                Alert.Show($"Не удалось сохранить конфигурацию\nпо причине: {ex.Message}");
            }
        }

        /// <summary>
        /// Считывает настройки из файла.
        /// </summary>
        public void Read()
        {
            try
            {
                // Спарсить файл конфигурации.
                JObject configFile = JObject.Parse(File.ReadAllText(configFilePath));
                // Присвоить значения из файла.
                ServerAddress = configFile["ServerAddress"].ToString();
                ServerPort = configFile["ServerPort"].ToString();
                ServerPath = configFile["ServerPath"].ToString();
                UseDarkTheme = configFile.Value<bool>("UseDarkTheme");
            }
            catch (Exception ex)
            {
                // Вывести ошибку.
                Alert.Show($"Не удалось открыть файл конфигурации\nпо причине: {ex.Message}\nБудет создана конфигурация по умолчанию.");
                // Создать и сохранить конфиг по умолчанию.
                config = CreateDefaultConfig();
                config.Save();
            }
        }

        /// <summary>
        /// Возвращает строку подключения.
        /// </summary>
        /// <returns>Строка подключения к серверу</returns>
        public string GetConnectionString()
        {
            return string.Format($"http://{ServerAddress}:{ServerPort}/{ServerPath}");
        }

        /// <summary>
        /// Установить тему оформления.
        /// </summary>
        public void SetTheme()
        {
            // Определяем путь к файлу ресурсов в зависимости от параметра темы.
            var uri = UseDarkTheme ? new Uri("Style\\DarkBrushes.xaml", UriKind.Relative) : new Uri("Style\\LightBrushes.xaml", UriKind.Relative);
            // Загружаем словарь ресурсов.
            ResourceDictionary resourceDict = Application.LoadComponent(uri) as ResourceDictionary;
            // Очищаем коллекцию ресурсов приложения.
            Application.Current.Resources.Clear();
            // Добавляем загруженный словарь ресурсов.
            Application.Current.Resources.MergedDictionaries.Add(resourceDict);
        }

        /// <summary>
        /// Создает конфигурацию по умолчанию.
        /// </summary>
        /// <returns>Результат выполнения</returns>
        private static JsonFileAppConfig CreateDefaultConfig()
        {
            // Создать объект конфигурации по умолчанию.
            return new JsonFileAppConfig();
        }

        /// <summary>
        /// Возвращает int представление версии программы.
        /// </summary>
        /// <returns>Int представление версии программы</returns>
        public static int GetAppBuild()
        {
            // Получить версию.
            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            // Вернуть сумму int частей версии.
            return version.Major * 1000 + version.Minor * 100 + version.Build * 10 + version.Revision;
        }

        /// <summary>
        /// Настройки приложения.
        /// </summary>
        private static JsonFileAppConfig config;
        /// <summary>
        /// Настройки приложения.
        /// </summary>
        public static JsonFileAppConfig Config
        {
            get
            {
                // Если конфиг не существует.
                if (config is null)
                {
                    // Если каталог настроек отсутствует, создать его.
                    if (!Directory.Exists(configPath))
                    {
                        Directory.CreateDirectory(configPath);
                    }
                    // Если файл конфигурации существует.
                    if (File.Exists(configFilePath))
                    {
                        // Создать конфигурацию по данным файла.
                        config = JsonConvert.DeserializeObject<JsonFileAppConfig>(File.ReadAllText(configFilePath));
                        // Если данные пустые.
                        if (string.IsNullOrWhiteSpace(config.ServerAddress)
                            || string.IsNullOrWhiteSpace(config.ServerPort)
                            || string.IsNullOrWhiteSpace(config.ServerPath))
                        {
                            // Создать и сохранить конфиг по умолчанию.
                            config = CreateDefaultConfig();
                            config.Save();
                        }
                    }
                    // Если файла нет.
                    else
                    {
                        // Создать и сохранить конфиг по умолчанию.
                        config = CreateDefaultConfig();
                        config.Save();
                    }
                }
                return config;
            }
        }
    }
}
