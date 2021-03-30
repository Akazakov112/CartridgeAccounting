using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using CartAccClient.Model;
using CartAccClient.View;
using CartAccLibrary.Dto;
using Microsoft.AspNetCore.SignalR.Client;

namespace CartAccClient.ViewModel
{
    /// <summary>
    /// ViewModel рабочей области.
    /// </summary>
    class WorkspaceVm : MainViewModel
    {
        private ListCollectionView ospsView;
        private OspDTO selectedOsp;
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
        /// Выбранное ОСП.
        /// </summary>
        public OspDTO SelectedOsp
        {
            get { return selectedOsp; }
            set { selectedOsp = value; RaisePropertyChanged(nameof(SelectedOsp)); }
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
        /// ViewModel области создания списаний.
        /// </summary>
        public CreateExpenseVm CreateExpenseVm { get; }

        /// <summary>
        /// ViewModel области баланса ОСП.
        /// </summary>
        public BalanceVm BalanceVm { get; }

        /// <summary>
        /// ViewModel области списаний ОСП.
        /// </summary>
        public ExpensesVm ExpensesVm { get; }

        /// <summary>
        /// ViewModel области поступлений ОСП.
        /// </summary>
        public ReceiptsVm ReceiptsVm { get; }

        /// <summary>
        /// ViewModel области поставщиков ОСП.
        /// </summary>
        public ProvidersVm ProvidersVm { get; }

        /// <summary>
        /// ViewModel области электронной почты ОСП.
        /// </summary>
        public EmailsVm EmailsVm { get; }

        /// <summary>
        /// ViewModel области словаря картриджей.
        /// </summary>
        public CartridgesVm CartridgesVm { get; }

        /// <summary>
        /// ViewModel области словаря принтеров.
        /// </summary>
        public PrintersVm PrintersVm { get; }

        /// <summary>
        /// ViewModel области управления ОСП.
        /// </summary>
        public OspsVm OspsVm { get; }

        /// <summary>
        /// ViewModel области управления пользователями.
        /// </summary>
        public UsersVm UsersVm { get; }

        /// <summary>
        /// ViewModel области отчета по движению картриджей.
        /// </summary>
        public MotionReportVm MotionReportVm { get; }

        /// <summary>
        /// Команда смены отображаемой области.
        /// </summary>
        public ICommand ChangeField
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    if (obj is string fieldName)
                    {
                        // Находим свойство ViewModel включаемого окна по переданному имени.
                        var vmEnableProp = this.GetType().GetProperty(fieldName);
                        // Получить значение этого свойства Vm в данном Vm.
                        var vmEnableValue = vmEnableProp.GetValue(this);
                        // Найти в полученном Vm свойство IsOpened и установить ему значение true.
                        vmEnableValue.GetType().GetProperty("IsOpened").SetValue(vmEnableValue, true);
                        // Перебрать открытые свойства данного Vm объекта.
                        foreach (var thisProps in this.GetType().GetProperties())
                        {
                            // Если имя свойства содержит метку Vm и не совпадает со свойством Vm вклюачемого окна.
                            if (thisProps.Name.Contains("Vm") && thisProps.Name != vmEnableProp.Name)
                            {
                                // Получить значение этого свойства Vm в данном Vm.
                                var valVm = thisProps.GetValue(this);
                                // Найти в полученном Vm свойство IsOpened и установить ему значение false.
                                valVm.GetType().GetProperty("IsOpened").SetValue(valVm, false);
                            }
                        }
                    }
                });
            }
        }

        /// <summary>
        /// Команда очистки лога.
        /// </summary>
        public ICommand ClearLog
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    // Очистить лог.
                    ServerConnect.ClearLog();
                });
            }
        }

        /// <summary>
        /// Команда подключения к другому ОСП.
        /// </summary>
        public ICommand ConnectToOsp
        {
            get
            {
                return new RelayCommand(async obj =>
                {
                    // Выполнить запрос на сервер с получением объекта пользователя по логину текущего пользователя системы.
                    await ServerConnect.Connection.InvokeAsync("GetOspData", Data.UserData.CurrentUser, SelectedOsp.Id);
                    SearchString = string.Empty;
                },
                // Доступно, если клиент подключен, выбрано ОСП для подключения и уровень доступа 4 или меньше (Пользователь).
                (canEx) => SelectedOsp != null && ServerConnect.Status && Data.UserData.CurrentUser.Access.Id <= 4);
            }
        }

        /// <summary>
        /// Команда открытия настроек.
        /// </summary>
        public ICommand Settings
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    // Открыть окно настроек.
                    new SettingsForm().ShowDialog();
                });
            }
        }

        /// <summary>
        /// Проверить обновления.
        /// </summary>
        public ICommand CheckUpdate
        {
            get
            {
                return new RelayCommand(async obj =>
                {
                    // Выполнить запрос обновления клиента на сервер.
                    await ServerConnect.Connection.InvokeAsync("CheckUpdate", JsonFileAppConfig.GetAppBuild());
                },
                // Доступно, если клиент подключен.
                (canEx) => ServerConnect.Status);
            }
        }

        /// <summary>
        /// Справка.
        /// </summary>
        public ICommand Help
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    try
                    {
                        Process.Start("https://address.help/");
                    }
                    catch(Exception ex)
                    {
                        Alert.Show($"Ошибка запуска справки\n{ex.Message}");
                    }
                });
            }
        }

        /// <summary>
        /// О программе.
        /// </summary>
        public ICommand About
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    Alert.Show("Автор: Казаков Андрей Александрович.\nООО \"Деловые Линии\", ОКС. 2020 г.", "Инфо о программе.", MessageBoxButton.OK);
                });
            }
        }

        /// <summary>
        /// Закрыть программу.
        /// </summary>
        public ICommand Exit
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    Application.Current.Shutdown();
                });
            }
        }

        /// <summary>
        /// Конструктор.
        /// </summary>
        public WorkspaceVm()
        {
            OspsView = new ListCollectionView(Data.UserData.Osps);
            CreateExpenseVm = new CreateExpenseVm();
            BalanceVm = new BalanceVm();
            ExpensesVm = new ExpensesVm();
            ReceiptsVm = new ReceiptsVm();
            ProvidersVm = new ProvidersVm();
            EmailsVm = new EmailsVm();
            CartridgesVm = new CartridgesVm();
            PrintersVm = new PrintersVm();
            OspsVm = new OspsVm();
            UsersVm = new UsersVm();
            MotionReportVm = new MotionReportVm();
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
