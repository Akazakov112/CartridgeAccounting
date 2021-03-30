using System;

namespace CartAccLibrary.Services
{
    /// <summary>
    /// Класс базового отчета.
    /// </summary>
    public abstract class Report
    {
        /// <summary>
        /// Дата создания.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Название ОСП отчета.
        /// </summary>
        public string OspName { get; set; }

        /// <summary>
        /// Конструктор базового отчета.
        /// </summary>
        /// <param name="date">Дата создания отчета</param>
        /// <param name="ospName">ОСП отчета</param>
        public Report(DateTime date, string ospName)
        {
            Date = date;
            OspName = ospName;
        }
    }
}
