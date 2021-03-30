using CartAccClient.Model;
using System.Windows;

namespace CartAccClient
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Обработчик старта программы.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // Проверка на уже запущенный экземпляр программы.
            if (RunOnlyOne.CheckRunProgram("CartAccClient")) 
                return;
            // Создать подключение к серверу.
            ConnectionServer.Create(JsonFileAppConfig.Config.GetConnectionString());
        }
    }
}
