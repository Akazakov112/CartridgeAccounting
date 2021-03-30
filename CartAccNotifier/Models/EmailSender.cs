using CartAccLibrary.Entities;
using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace CartAccNotifier.Models
{
    /// <summary>
    /// Проводит оповещение по электронной почте.
    /// </summary>
    class EmailSender
    {
        /// <summary>
        /// Клиент Smtp сервера.
        /// </summary>
        private SmtpClient Client { get; }

        /// <summary>
        /// Отправитель.
        /// </summary>
        private readonly string sender;

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="server">Адрес сервера</param>
        /// <param name="port">Порт сервера</param>
        /// <param name="senderAddress">Адрес отправителя</param>
        public EmailSender(string server, int port, string senderAddress)
        {
            Client = new SmtpClient(server, port)
            {
                Credentials = CredentialCache.DefaultNetworkCredentials,
                EnableSsl = false
            };
            sender = senderAddress;
        }


        /// <summary>
        /// Оповещает ОСП.
        /// </summary>
        /// <param name="osp">ОСП</param>
        /// <returns></returns>
        public async Task NotifyAsync(NotifyOsp osp)
        {
            // Найти первый баланс в списке.
            Balance firstBalance = osp.Balances.FirstOrDefault();
            if (firstBalance != null)
            {
                // Список адресов для уведомления.
                string emails = osp.Emails.Count > 1 ? string.Join(',', osp.Emails.Select(x => x.Address)) : osp.Emails.FirstOrDefault().Address;
                // Тема письма.
                string subject = $"Уведомление об остатках картриджей в ОСП {osp.Name}";
                // Формирование тела письма.
                string body = $"<h4>В ОСП {osp.Name} на {DateTime.Today:dd.MM.yyyy} следующие картриджи имеют низкий остаток.</h4>" +
                    $"<table border=\"1\" cellpadding=\'5\'>" +
                    $"<tr><th>№</th><th>Модель</th><th>Количество</th></tr>";
                for (int i = 0; i < osp.Balances.Count; i++)
                {
                    body += $"<tr><td>{i + 1}</td><td>{osp.Balances[i].Cartridge.Model}</td><td>{osp.Balances[i].Count}</td></tr>\n";
                }
                body += "<//table>";
                // Создаать сообщение.
                MailMessage message = new MailMessage(sender, emails, subject, body)
                {
                    IsBodyHtml = true
                };
                await Client.SendMailAsync(message);
            }
        }
    }
}
