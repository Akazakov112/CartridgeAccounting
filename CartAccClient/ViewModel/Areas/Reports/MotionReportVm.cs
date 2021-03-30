using CartAccClient.Model;
using CartAccClient.View;
using CartAccLibrary.Dto;
using CartAccLibrary.Services;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace CartAccClient.ViewModel
{
    /// <summary>
    /// ViewModel вкладки отчета по движению картриджей.
    /// </summary>
    class MotionReportVm : MainViewModel
    {
        private ListCollectionView ospsView;
        private ExcelFileBuilder builder;
        private MotionReport selectedReport;
        private string searchString;

        /// <summary>
        /// Представление коллекции ОСП.
        /// </summary>
        public ListCollectionView OspsView
        {
            get { return ospsView; }
            set { ospsView = value; RaisePropertyChanged(nameof(OspsView)); }
        }

        /// <summary>
        /// Строка поискового запроса.
        /// </summary>
        public string SearchString
        {
            get { return searchString; }
            set
            {
                searchString = value;
                RaisePropertyChanged(nameof(SearchString));
                OspsView.Filter = new Predicate<object>(Filtering);
            }
        }

        /// <summary>
        /// Построитель файла Excel.
        /// </summary>
        public ExcelFileBuilder Builder
        {
            get { return builder; }
            set { builder = value; RaisePropertyChanged(nameof(Builder)); }
        }

        /// <summary>
        /// Начало периода.
        /// </summary>
        public DateTime Start { get; set; }

        /// <summary>
        /// Конец периода.
        /// </summary>
        public DateTime End { get; set; }

        /// <summary>
        /// Количество дней для активных картриджей.
        /// </summary>
        public int Days { get; set; }

        /// <summary>
        /// Отчеты по ОСП.
        /// </summary>
        public ObservableCollection<MotionReport> Reports { get; set; }

        /// <summary>
        /// ОСП для отчетов.
        /// </summary>
        public ObservableCollection<OspDTO> AddedOsps { get; set; }

        /// <summary>
        /// Выбранный отчет.
        /// </summary>
        public MotionReport SelectedReport
        {
            get { return selectedReport; }
            set { selectedReport = value; RaisePropertyChanged(nameof(SelectedReport)); }
        }

        /// <summary>
        /// Выбранное добавляемое ОСП.
        /// </summary>
        public OspDTO SelectedAddingOsp { get; set; }

        /// <summary>
        /// Выбранное добавленое ОСП.
        /// </summary>
        public OspDTO SelectedAddedOsp { get; set; }

        /// <summary>
        /// Добавлить ОСП в список.
        /// </summary>
        public ICommand AddOsp
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    if (!AddedOsps.Contains(SelectedAddingOsp))
                    {
                        AddedOsps.Add(SelectedAddingOsp);
                    }
                    else
                    {
                        Alert.Show("ОСП уже добавлено!", "Добавление ОСП", MessageBoxButton.OK);
                    }
                },
                (canEx) => SelectedAddingOsp != null);
            }
        }

        /// <summary>
        /// Удалить ОСП из списка.
        /// </summary>
        public ICommand DelOsp
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    AddedOsps.Remove(SelectedAddedOsp);
                },
                (canEx) => SelectedAddedOsp != null);
            }
        }

        /// <summary>
        /// Добавить все ОСП в список.
        /// </summary>
        public ICommand AddAllOsps
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    AddedOsps.Clear();
                    foreach (var osp in Data.UserData.Osps.Where(x => x.Active))
                    {
                        AddedOsps.Add(osp);
                    }
                },
                (canEx) => Data.UserData.Osps.Any());
            }
        }

        /// <summary>
        /// Удаляет все ОСП.
        /// </summary>
        public ICommand DelAllOsps
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    AddedOsps.Clear();
                },
                (canEx) => AddedOsps.Any());
            }
        }

        /// <summary>
        /// Сформировать отчет.
        /// </summary>
        public ICommand GetReport
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    ServerConnect.Connection.InvokeAsync("GetMotionReports", AddedOsps.ToArray(), Start, End, Days);
                },
                // Доступно, если есть коннект, добавлены ОСП и количество дней актуальности картриджей больше 0.
                (canEx) => ServerConnect.Status && AddedOsps.Any() && Days > 0);
            }
        }

        /// <summary>
        /// Очистить отчет.
        /// </summary>
        public ICommand ClearReport
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    Reports.Clear();
                });
            }
        }

        /// <summary>
        /// Экспорт выбранного отчета в Excel файл.
        /// </summary>
        public ICommand ExportToExcel
        {
            get
            {
                return new RelayCommand(async obj =>
                {
                    // Диалог сохранения файла.
                    SaveFileDialog sfd = new SaveFileDialog()
                    {
                        // Параметры диалога.
                        DefaultExt = "*.xlsx",
                        Filter = "Excel 2007 + |*.xlsx",
                        Title = "Сохранение в Excel"
                    };
                    if (sfd.ShowDialog() == true)
                    {
                        Builder = new ExcelFileBuilder(sfd.FileName, new List<MotionReport>() { SelectedReport });
                        string result = await Builder.SaveFileAsync() ? "Отчет успешно сохранен." : "Ошибка сохранения отчета.";
                        // Вывести результат сохранения.
                        Alert.Show(result, "Сохранения отчета", MessageBoxButton.OK);
                        // Сбросить прогресс бар.
                        Builder.CurrentProgress = 0;
                    }
                },
                // Доступно, когда выбран отчет.
                (canEx) => SelectedReport != null);
            }
        }

        /// <summary>
        /// Экспорт все отчеты в Excel файл.
        /// </summary>
        public ICommand ExportAllToExcel
        {
            get
            {
                return new RelayCommand(async obj =>
                {
                    // Диалог сохранения файла.
                    SaveFileDialog sfd = new SaveFileDialog()
                    {
                        // Параметры диалога.
                        DefaultExt = "*.xlsx",
                        Filter = "Excel 2007 + |*.xlsx",
                        Title = "Сохранение в Excel"
                    };
                    if (sfd.ShowDialog() == true)
                    {
                        Builder = new ExcelFileBuilder(sfd.FileName, Reports.ToList());
                        string result = await Builder.SaveFileAsync() ? "Отчет успешно сохранен." : "Ошибка сохранения отчета.";
                        // Вывести результат сохранения.
                        Alert.Show(result, "Сохранения отчета", MessageBoxButton.OK);
                        // Сбросить прогресс бар.
                        Builder.CurrentProgress = 0;
                    }
                },
                // Доступно, когда есть отчеты.
                (canEx) => Reports.Any());
            }
        }

        /// <summary>
        /// Конструктор.
        /// </summary>
        public MotionReportVm()
        {
            Start = DateTime.Today.AddDays(-30);
            End = DateTime.Today;
            Days = 180;
            AddedOsps = new ObservableCollection<OspDTO>();
            Reports = new ObservableCollection<MotionReport>();
            OspsView = new ListCollectionView(Data.UserData.Osps.Where(x => x.Active).ToList());

            // Обработчик вызова получения отчетов.
            ServerConnect.Connection.On<IEnumerable<MotionReport>>("UpdateReports", (reports) =>
            {
                Reports.Clear();
                foreach (var report in reports)
                {
                    Reports.Add(report);
                }
                SelectedReport = Reports.FirstOrDefault();
            });
        }


        /// <summary>
        /// Метод фильтрации.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private bool Filtering(object obj)
        {
            // Если объекты не null.
            if (obj is OspDTO item)
            {
                return item.Name.ToLower().Contains(SearchString.ToLower());
            }
            else
            {
                return false;
            }
        }
    }
}
