using CartAccLibrary.Services;
using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace CartAccClient.Model
{
    /// <summary>
    /// Построитель файла Excel для сохранения отчетов.
    /// </summary>
    class ExcelFileBuilder : BaseVm
    {
        private int maxProgress;
        private int currentProgress;

        /// <summary>
        /// Путь к файлу сохранения.
        /// </summary>
        private string Filepath { get; }

        /// <summary>
        /// Сохраняемый отчет.
        /// </summary>
        private List<MotionReport> Reports { get; }

        /// <summary>
        /// Максимум прогресса выполнения.
        /// </summary>
        public int MaxProgress
        {
            get { return maxProgress; }
            set { maxProgress = value; RaisePropertyChanged(nameof(MaxProgress)); }
        }

        /// <summary>
        /// Прогресс выпонения.
        /// </summary>
        public int CurrentProgress
        {
            get { return currentProgress; }
            set { currentProgress = value; RaisePropertyChanged(nameof(CurrentProgress)); }
        }

        /// <summary>
        /// Конструктор с параметрами.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="reports"></param>
        public ExcelFileBuilder(string filePath, List<MotionReport> reports)
        {
            Filepath = filePath;
            Reports = reports;
            MaxProgress = reports.Select(x=>x.Cartridges).Count();
            CurrentProgress = 1;
        }


        /// <summary>
        /// Сохраняет отчет в файл асинхронно.
        /// </summary>
        public async Task<bool> SaveFileAsync()
        {
            return await Task.Run(() => SaveFile());
        }

        /// <summary>
        /// Сохраняет отчет в файл.
        /// </summary>
        private bool SaveFile()
        {
            try
            {
                // Экземпляр книги.
                using (XLWorkbook wb = new XLWorkbook())
                {
                    foreach (var report in Reports)
                    {
                        // Вкладка с названием по ОСП отчета с датой.
                        IXLWorksheet ws = wb.Worksheets.Add($"{report.OspName}");
                        // Вставить таблицу.
                        ws.Cell(1, 1).InsertTable(GetTable(report));
                        // Установить автоподбор ширины столбцов.
                        ws.Columns().AdjustToContents();
                    }
                    // Сохранения файла.
                    wb.SaveAs(Filepath);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Создает DataTable по отчету.
        /// </summary>
        /// <returns></returns>
        private DataTable GetTable(MotionReport report)
        {
            var table = new DataTable();
            table.Columns.Add("№", typeof(int));
            table.Columns.Add("Модель", typeof(string));
            table.Columns.Add("Списано", typeof(int));
            table.Columns.Add("Поступило", typeof(int));
            table.Columns.Add($"Остаток на {DateTime.Today:dd.MM.yyyy}", typeof(int));
            foreach (var cartridge in report.Cartridges)
            {
                table.Rows.Add(cartridge.Number, cartridge.Model, cartridge.ExpenseCount, cartridge.ReceiptCount, cartridge.BalanceCount);
                CurrentProgress++;
            }
            return table;
        }
    }
}
