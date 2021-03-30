namespace CartAccServer.Models.Interfaces.Infrastructure
{
    /// <summary>
    /// Интерфейс для блокируемых документов.
    /// </summary>
    public interface IBlockedDocument
    {
        /// <summary>
        /// Id документа.
        /// </summary>
        int DocumentId { get; set; }

        /// <summary>
        /// Id ОСП нахождения документа.
        /// </summary>
        int OspId { get; set; }

        /// <summary>
        /// Тип документа.
        /// </summary>
        string Type { get; set; }

        /// <summary>
        /// Пользователь, заблокировавший документ.
        /// </summary>
        string User { get; set; }
    }
}
