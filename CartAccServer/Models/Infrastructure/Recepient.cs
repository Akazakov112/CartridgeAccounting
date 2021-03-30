namespace CartAccServer.Models.Infrastructure
{
    /// <summary>
    /// Получатель сообщения.
    /// </summary>
    public class Recepient
    {
        /// <summary>
        /// Id подключения.
        /// </summary>
        public string ConnectionId { get; set; }

        /// <summary>
        /// Имя получателя.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Конструктор.
        /// </summary>
        public Recepient(string connectionId, string name)
        {
            ConnectionId = connectionId;
            Name = name;
        }
    }
}
