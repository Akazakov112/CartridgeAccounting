using CartAccLibrary.Dto;
using CartAccLibrary.Entities;

namespace CartAccServer.Models.Interfaces.Services
{
    /// <summary>
    /// Интерфейс общего сервиса для работы с сущностями.
    /// </summary>
    public interface IDataService
    {
        IEntityService<Access, AccessDTO> Accesses { get; }

        IEntityService<Cartridge, CartridgeDTO> Cartridges { get; }

        IEntityService<Printer, PrinterDTO> Printers { get; }

        IEntityService<Osp, OspDTO> Osps { get; }

        IEntityService<Balance, BalanceDTO> Balance { get; }

        IEntityService<Email, EmailDTO> Emails { get; }

        IEntityService<Expense, ExpenseDTO> Expenses { get; }

        IEntityService<Provider, ProviderDTO> Providers { get; }

        IEntityService<Receipt, ReceiptDTO> Receipts { get; }

        IEntityService<User, UserDTO> Users { get; }

        OspDataDTO GetOspData(int userId, int ospId);
    }
}
