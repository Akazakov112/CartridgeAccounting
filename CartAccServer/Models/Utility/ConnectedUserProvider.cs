using System.Collections.Generic;
using CartAccServer.Models.Infrastructure;
using CartAccServer.Models.Interfaces.Infrastructure;
using CartAccServer.Models.Interfaces.Utility;

namespace CartAccServer.Models.Utility
{
    /// <summary>
    /// Поставщик работы с подключенными пользователями.
    /// </summary>
    public class ConnectedUserProvider : IConnectedUserProvider
    {
        public List<IConnectedUser> ConnectedUsers { get; }

        public ConnectedUserProvider()
        {
            ConnectedUsers = new List<IConnectedUser>();
        }


        public void AddUser(int dbId, string connectionId, string name, string osp)
        {
            if (ConnectedUsers.FindAll(x => x.ConnectionId == connectionId).Count == 0)
            {
                var connectedUser = new ConnectedUser(dbId, connectionId, name, osp);
                ConnectedUsers.Add(connectedUser);
            }
        }

        public void RemoveUser(string connectionId)
        {
            if (!string.IsNullOrWhiteSpace(connectionId))
            {
                ConnectedUsers.RemoveAll(x => x.ConnectionId == connectionId);
            }
        }
    }
}
