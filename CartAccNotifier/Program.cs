using CartAccNotifier.Models;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace CartAccNotifier
{
    class Program
    {
        [SuppressMessage("Style", "IDE0060:Удалите неиспользуемый параметр", Justification = "<Ожидание>")]
        static async Task Main(string[] args)
        {
            // Чтение файла конфигурации.
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile($"Config/Configuration.json", true, true)
                .Build();

            // Минимальное количество остатка картриджа для уведомления.
            int minBalanceCount = int.Parse(config["MinBalanceCount"]);

            // Адрес почтового сервера.
            string serverAddress = config["SmtpServer"];

            // Порт почтового сервера.
            int serverPort = int.Parse(config["SmtpPort"]);

            // Адрес почты отправителя.
            string senderAddress = config["SenderAddress"];

            // Построитель данных.
            var dataBuilder = new DataBuilder(config.GetConnectionString("DbConnection"));

            // Отправитель уведомлений.
            var emailSender = new EmailSender(serverAddress, serverPort, senderAddress);

            // Получить список ОСП для уведомлений.
            List<NotifyOsp> osps = dataBuilder.GetOspForNotify(minBalanceCount);

            // Отправить уведомления.
            foreach (var osp in osps)
            {
                await emailSender.NotifyAsync(osp);
            }
        }
    }
}
