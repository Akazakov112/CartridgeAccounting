using System.Collections.Generic;
using CartAccServer.Models.Interfaces.Infrastructure;

namespace CartAccServer.Models.Interfaces.Utility
{
    /// <summary>
    /// Поставщик работы с подключенными пользователями.
    /// </summary>
    public interface IConnectedUserProvider
    {
        /// <summary>
        /// Список подключенных пользователей.
        /// </summary>
        List<IConnectedUser> ConnectedUsers { get; }

        /// <summary>
        /// Добавить пользователя.
        /// </summary>
        void AddUser(int dbId, string connectionId, string name, string osp);

        /// <summary>
        /// Удалить пользователя.
        /// </summary>
        void RemoveUser(string connectionId);
    }
}
