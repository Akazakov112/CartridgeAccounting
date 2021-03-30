using CartAccLibrary.Services;

namespace CartAccLibrary.Dto
{
    /// <summary>
    /// DTO класс картриджа поступления.
    /// </summary>
    public class ReceiptCartridgeDTO : BaseVm
    {
        /// <summary>
        /// Взятый картридж.
        /// </summary>
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
        public int Count { get; set; }

        /// <summary>
        /// Пустой конструктор.
        /// </summary>
        public ReceiptCartridgeDTO() { }

        /// <summary>
        /// Полный конструктор.
        /// </summary>
        /// <param name="id">Id записи картриджа поступления</param>
        /// <param name="cartridge">Картридж</param>
        /// <param name="count">Количество</param>
        public ReceiptCartridgeDTO(int id, CartridgeDTO cartridge, int count)
        {
            Id = id;
            Cartridge = cartridge;
            Count = count;
        }
    }
}
