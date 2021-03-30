namespace CartAccServer.Models.Interfaces.Infrastructure
{
    /// <summary>
    /// Интерфейс для подключенного пользователя.
    /// </summary>
    public interface IConnectedUser
    {
        /// <summary>
        /// Id в базе данных.
        /// </summary>
        int DbId { get; }

        /// <summary>
        /// Id подключения.
        /// </summary>
        string ConnectionId { get; }

        /// <summary>
        /// Имя.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Название ОСП.
        /// </summary>
        string Osp { get; set; }

        /// <summary>
        /// Версия клиента.
        /// </summary>
        int ClientVersion { get; set; }
    }
}
