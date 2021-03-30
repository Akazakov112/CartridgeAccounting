using CartAccServer.Models.Interfaces.Infrastructure;

namespace CartAccServer.Models.Interfaces.Services
{
    /// <summary>
    /// Интерфейс сервиса блокировки документов для изменений.
    /// </summary>
    public interface IBlockedDocumentService
    {
        /// <summary>
        /// Проверить блокировку документа по параметрам.
        /// </summary>
        /// <param name="documentId">Id документа</param>
        /// <param name="ospId">Id ОСП</param>
        /// <param name="type">Тип</param>
        /// <param name="user">Пользователь</param>
        /// <returns>Заблокированный документ</returns>
        IBlockedDocument CheckBlock(int documentId, string type, int ospId);

        /// <summary>
        /// Установить блок на документ.
        /// </summary>
        /// <param name="document"></param>
        void SetBlock(int documentId, int ospId, string type, string user);

        /// <summary>
        /// Снять блок с документа.
        /// </summary>
        /// <param name="document">Документ</param>
        void UnsetBlock(IBlockedDocument document);

        /// <summary>
        /// Снять все блоки с документа по имени пользователя.
        /// </summary>
        /// <param name="username">Имя пользователя</param>
        void UnsetBlock(string username);
    }
}
