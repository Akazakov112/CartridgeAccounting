using System.Collections.Generic;
using System.Linq;
using CartAccServer.Models.Interfaces.Infrastructure;

namespace CartAccServer.ViewModel
{
    /// <summary>
    /// ViewModel представления статистики.
    /// </summary>
    public class StatisticVm
    {
        /// <summary>
        /// Подключенные пользователи.
        /// </summary>
        public List<IConnectedUser> Users { get; }

        /// <summary>
        /// Количество подключенных пользователей.
        /// </summary>
        public int AllUsersCount { get; }

        /// <summary>
        /// Количество ОСП, в которых подключены пользователи.
        /// </summary>
        public int OspCount { get; }

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="users">Подключенные пользователи</param>
        public StatisticVm(List<IConnectedUser> users)
        {
            Users = users;
            AllUsersCount = Users.Count;
            OspCount = Users.Select(x => x.Osp).Distinct().Count();
        }
    }
}
