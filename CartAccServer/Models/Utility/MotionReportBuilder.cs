using System;
using System.Collections.Generic;
using System.Linq;
using CartAccLibrary.Dto;
using CartAccLibrary.Services;
using CartAccServer.Models.Infrastructure;
using CartAccServer.Models.Interfaces.Services;
using CartAccServer.Models.Interfaces.Utility;

namespace CartAccServer.Models.Utility
{
    /// <summary>
    /// Построитель отчетов по движению картриджей.
    /// </summary>
    public class MotionReportBuilder : IReportBuilder
    {
        /// <summary>
        /// Сервис работы с данными.
        /// </summary>
        private IDataService DataService { get; }

        /// <summary>
        /// Начало периода отчета.
        /// </summary>
        private DateTime StartPeriod { get; }

        /// <summary>
        /// Конец периода отчета.
        /// </summary>
        private DateTime EndPeriod { get; }

        /// <summary>
        /// ОСП отчета.
        /// </summary>
        private OspDTO Osp { get; }

        /// <summary>
        /// Количество дней для актуализации картриджей.
        /// </summary>
        private int ActualCartDays { get; }

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="osp">ОСП отчета</param>
        /// <param name="start">Начало периода отчета</param>
        /// <param name="end">Конец периода отчета</param>
        /// <param name="actualCartDays">Дни актуальности картриджа</param>
        public MotionReportBuilder(IDataService dataService, OspDTO osp, DateTime start, DateTime end, int actualCartDays)
        {
            DataService = dataService;
            StartPeriod = start;
            EndPeriod = end;
            Osp = osp;
            ActualCartDays = actualCartDays;
        }

        /// <summary>
        /// Создает отчет ОСП по движению картриджей.
        /// </summary>
        /// <returns>Отчет ОСП</returns>
        public Report Create()
        {
            // Коллекция картриджей для отчета.
            List<MotionCartridgeDTO> cartridges = new List<MotionCartridgeDTO>();
            // Список Id актуальных картриджей в списаниях.
            int[] expenseActualCartId = DataService.Expenses
                .Find(x => x.Osp.Id == Osp.Id && x.Date >= DateTime.Today.AddDays(-ActualCartDays))
                .Select(u => u.Cartridge.Id)
                .ToArray();
            // Список Id актуальных картриджей в поступлениях.
            int[] recActualCartId = DataService.Receipts
                .Find(x => x.Osp.Id == Osp.Id && x.Date >= DateTime.Today.AddDays(-ActualCartDays))
                .SelectMany(u => u.Cartridges)
                .Select(c => c.Cartridge.Id)
                .Distinct()
                .ToArray();
            // Объединение id актуальных картриджей
            int[] actualCartsId = expenseActualCartId.Union(recActualCartId).ToArray();
            // Счетчик номера.
            int number = 1;
            // Перебрать Id актуальных картриджей.
            foreach (var cartId in actualCartsId)
            {
                // Сумма списаний картриджа.
                int expenseCount = DataService.Expenses
                    .Find(x => x.Osp.Id == Osp.Id && x.Date >= StartPeriod && x.Date <= EndPeriod && x.Cartridge.Id == cartId)
                    .Select(c => c.Count)
                    .ToArray()
                    .Sum();
                // Сумма поступлений картриджа.
                int receiptsCount = DataService.Receipts
                    .Find(x => x.Osp.Id == Osp.Id && x.Date >= StartPeriod && x.Date <= EndPeriod)
                    .SelectMany(u => u.Cartridges)
                    .Where(c => c.Cartridge.Id == cartId)
                    .Select(t => t.Count)
                    .ToArray()
                    .Sum();
                // Текущий остаток.
                int balanceCount = DataService.Balance.Find(x => x.Osp.Id == Osp.Id && x.Cartridge.Id == cartId).FirstOrDefault().Count;
                // Модель картриджа.
                string model = DataService.Cartridges.Get(cartId).Model;
                // Создать картридж отчета.
                MotionCartridgeDTO cartridge = new MotionCartridgeDTO(number, model, expenseCount, receiptsCount, balanceCount);
                // Добавить в список.
                cartridges.Add(cartridge);
                // Увеличить счетчик.
                number++;
            }
            if (cartridges.Any())
            {
                // Создать отчет.
                MotionReport report = new MotionReport(DateTime.Today, Osp.Name, cartridges);
                return report;
            }
            else
            {
                throw new ValidationException($"По ОСП {Osp.Name} отсутствуют движения картриджей.", "");
            }
        }
    }
}
