using CartAccLibrary.Services;

namespace CartAccLibrary.Dto
{
    /// <summary>
    /// DTO класс баланса.
    /// </summary>
    public class BalanceDTO : BaseVm
    {
        private int count;
        private bool inUse;
        private CartridgeDTO cartridge;


        /// <summary>
        /// Id.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Картридж.
        /// </summary>
        public CartridgeDTO Cartridge
        {
            get { return cartridge; }
            set { cartridge = value; RaisePropertyChanged(nameof(Cartridge)); }
        }

        /// <summary>
        /// Количество.
        /// </summary>
        public int Count
        {
            get { return count; }
            set { count = value; RaisePropertyChanged(nameof(Count)); }
        }

        /// <summary>
        /// Статус использования.
        /// </summary>
        public bool InUse 
        {
            get { return inUse; }
            set { inUse = value; RaisePropertyChanged(nameof(InUse)); }
        }

        /// <summary>
        /// Id ОСП расположения.
        /// </summary>
        public int OspId { get; set; }

        /// <summary>
        /// Пустой конструктор.
        /// </summary>
        public BalanceDTO() { }

        /// <summary>
        /// Полный конструктор.
        /// </summary>
        /// <param name="id">Id баланса</param>
        /// <param name="number">Номер</param>
        /// <param name="cartridge">Картридж</param>
        /// <param name="count">Количество</param>
        public BalanceDTO(int id, CartridgeDTO cartridge, int count, bool inUser, int ospId)
        {
            Id = id;
            Cartridge = cartridge;
            Count = count;
            InUse = inUser;
            OspId = ospId;
        }
    }
}
