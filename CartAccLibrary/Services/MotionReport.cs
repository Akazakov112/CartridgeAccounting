using CartAccLibrary.Dto;
using System;
using System.Collections.Generic;

namespace CartAccLibrary.Services
{
    /// <summary>
    /// Отчет по движению картриджей.
    /// </summary>
    public class MotionReport : Report
    {
        /// <summary>
        /// Список картриджей в отчете.
        /// </summary>
        public List<MotionCartridgeDTO> Cartridges { get; set; }

        /// <summary>
        /// Конструктор.
        /// </summary>
        public MotionReport(DateTime date, string ospName, List<MotionCartridgeDTO> cartridges) : base(date, ospName) 
        {
            Cartridges = cartridges;
        }
    }
}
