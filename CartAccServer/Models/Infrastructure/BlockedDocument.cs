using CartAccServer.Models.Interfaces.Infrastructure;

namespace CartAccServer.Models.Infrastructure
{
    /// <summary>
    /// Заблокированный для редактирования документ.
    /// </summary>
    public class BlockedDocument : IBlockedDocument
    {
        /// <summary>
        /// Id документа.
        /// </summary>
        public int DocumentId { get; set; }

        /// <summary>
        /// Id ОСП нахождения документа.
        /// </summary>
        public int OspId { get; set; }

        /// <summary>
        /// Тип документа.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Пользователь, заблокировавший документ.
        /// </summary>
        public string User { get; set; }

        /// <summary>
        /// Полный конструктор.
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="type">Тип</param>
        /// <param name="ospId">ОСП нахождения</param>
        /// <param name="user">Пользователь</param>
        public BlockedDocument(int documentId, int ospId, string type, string user)
        {
            DocumentId = documentId;
            Type = type;
            OspId = ospId;
            User = user;
        }
    }
}
