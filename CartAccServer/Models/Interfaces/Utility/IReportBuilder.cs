using CartAccLibrary.Services;

namespace CartAccServer.Models.Interfaces.Utility
{
    /// <summary>
    /// Интерфейс создателя отчетов.
    /// </summary>
    interface IReportBuilder
    {
        /// <summary>
        /// Создает отчет.
        /// </summary>
        /// <returns>Отчет.</returns>
        Report Create();
    }
}
