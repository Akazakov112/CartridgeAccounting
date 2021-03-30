using CartAccLibrary.Entities;
using System.Collections.Generic;

namespace CartAccNotifier.Models
{
    /// <summary>
    /// ОСП для оповещения.
    /// </summary>
    class NotifyOsp
    {
        /// <summary>
        /// Наименование ОСП.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Баланс.
        /// </summary>
        public List<Balance> Balances { get; }

        /// <summary>
        /// Электронные адреса для уведомлений.
        /// </summary>
        public List<Email> Emails { get; }

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="name">Название</param>
        /// <param name="balances">Баланс</param>
        /// <param name="emails">Электронные адреса для уведомлений</param>
        public NotifyOsp(string name, List<Balance> balances, List<Email> emails)
        {
            Name = name;
            Balances = balances;
            Emails = emails;
        }
    }
}
