namespace CartAccLibrary.Dto
{
    /// <summary>
    /// Картридж для отчета по движению.
    /// </summary>
    public class MotionCartridgeDTO
    {
        /// <summary>
        /// Порядковый номер.
        /// </summary>
        public int Number { get; set; }

        /// <summary>
        /// Модель.
        /// </summary>
        public string Model { get; set; }

        /// <summary>
        /// Количество списанных.
        /// </summary>
        public int ExpenseCount { get; set; }

        /// <summary>
        /// Количество поступивших.
        /// </summary>
        public int ReceiptCount { get; set; }

        /// <summary>
        /// Количество на остатке.
        /// </summary>
        public int BalanceCount { get; set; }

        /// <summary>
        /// Конструктор.
        /// </summary>
        public MotionCartridgeDTO() { }

        /// <summary>
        /// Контсруктор.
        /// </summary>
        /// <param name="number">Порядковый номер</param>
        /// <param name="model">Модель</param>
        /// <param name="expCount">Количество списанных</param>
        /// <param name="recCount">Количество поступивших</param>
        /// <param name="balanceCount">Количество на остатке</param>
        public MotionCartridgeDTO(int number, string model, int expCount, int recCount, int balanceCount)
        {
            Number = number;
            Model = model;
            ExpenseCount = expCount;
            ReceiptCount = recCount;
            BalanceCount = balanceCount;
        }
    }
}
