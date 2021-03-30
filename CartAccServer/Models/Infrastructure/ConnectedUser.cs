using CartAccServer.Models.Interfaces.Infrastructure;

namespace CartAccServer.Models.Infrastructure
{
    /// <summary>
    /// Подключенный к серверу пользователь.
    /// </summary>
    public class ConnectedUser : IConnectedUser
    {
        public int DbId { get; }

        public string ConnectionId { get; }

        public string Name { get; set; }

        public string Osp { get; set; }

        public int ClientVersion { get; set; }

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="dbId">Id в базе данных</param>
        /// <param name="connectionId">Id подключения</param>
        /// <param name="name">Имя</param>
        /// <param name="osp">Название ОСП</param>
        public ConnectedUser(int dbId, string connectionId, string name, string osp)
        {
            DbId = dbId;
            ConnectionId = connectionId;
            Name = name;
            Osp = osp;
            ClientVersion = 1000;
        }
    }
}
