using System.Collections.Generic;
using System.Linq;
using CartAccServer.Models.Infrastructure;
using CartAccServer.Models.Interfaces.Infrastructure;
using CartAccServer.Models.Interfaces.Services;

namespace CartAccServer.Models.Services
{
    /// <summary>
    /// Сервис блокировки документов для редактирования.
    /// </summary>
    public class BlockedDocumentService : IBlockedDocumentService
    {
        /// <summary>
        /// Список заблокированных документов.
        /// </summary>
        private List<BlockedDocument> BlockedDocuments { get; }

        /// <summary>
        /// Конструктор.
        /// </summary>
        public BlockedDocumentService()
        {
            BlockedDocuments = new List<BlockedDocument>();
        }


        /// <summary>
        /// Проверить блокировку документа по параметрам.
        /// </summary>
        /// <param name="documentId">Id документа</param>
        /// <param name="ospId">Id ОСП</param>
        /// <param name="type">Тип</param>
        /// <param name="user">Пользователь</param>
        /// <returns>Заблокированный документ</returns>
        public IBlockedDocument CheckBlock(int documentId, string type, int ospId)
        {
            BlockedDocument blockDoc = BlockedDocuments.Where(x => x.DocumentId == documentId && x.OspId == ospId && x.Type == type).FirstOrDefault();
            return blockDoc;
        }

        /// <summary>
        /// Установить блок на документ.
        /// </summary>
        /// <param name="document"></param>
        public void SetBlock(int documentId, int ospId, string type, string user)
        {
            // Создать дкоумент блокировки.
            var document = new BlockedDocument(documentId, ospId, type, user);
            // Добавить в список заблокированных.
            BlockedDocuments.Add(document);
        }

        /// <summary>
        /// Снять блок с документа.
        /// </summary>
        /// <param name="document">Документ</param>
        public void UnsetBlock(IBlockedDocument document)
        {
            if (document is BlockedDocument doc)
                BlockedDocuments.Remove(doc);
        }

        /// <summary>
        /// Снять все блоки с докмента по имени пользователя.
        /// </summary>
        /// <param name="username">Имя пользователя</param>
        public void UnsetBlock(string username)
        {
            // Если переданное имя пользователя пустое или состоит из пробелов.
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ValidationException("Имя пользователя не должно быть пустым!", "");
            }
            // Иначе удалить все документы с переданным именем пользователя.
            else
            {
                BlockedDocuments.RemoveAll(x => x.User == username);
            }
        }
    }
}
