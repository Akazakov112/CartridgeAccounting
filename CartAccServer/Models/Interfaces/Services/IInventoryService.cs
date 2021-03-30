using System.Collections.Generic;
using CartAccLibrary.Dto;

namespace CartAccServer.Models.Interfaces.Services
{
    /// <summary>
    /// Интерфейс сервиса проведения инвентаризации.
    /// </summary>
    public interface IInventoryService
    {
        /// <summary>
        /// Списания по результатам инвентаризации.
        /// </summary>
        List<ExpenseDTO> Expenses { get; }

        /// <summary>
        /// Поступление по результатам инвентаризации.
        /// </summary>
        ReceiptDTO Receipt { get; }
    }
}
