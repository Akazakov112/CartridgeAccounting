namespace CartAccLibrary.Dto
{
    /// <summary>
    /// Баланс для инвентаризации.
    /// </summary>
    public class InventBalanceDTO : BalanceDTO
    {
        /// <summary>
        /// Фактическое количество на складе.
        /// </summary>
        public uint? FactCount { get; set; }

        /// <summary>
        /// Конструктор.
        /// </summary>
        public InventBalanceDTO() { }
    }
}
