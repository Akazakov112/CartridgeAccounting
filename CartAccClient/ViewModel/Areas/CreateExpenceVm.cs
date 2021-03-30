using CartAccClient.Model;
using CartAccClient.View;
using CartAccLibrary.Dto;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace CartAccClient.ViewModel
{
    /// <summary>
    /// ViewModel области создания списаний.
    /// </summary>
    class CreateExpenseVm : MainViewModel
    {
        private ListCollectionView cartridgeView, printersView;
        private PrinterDTO selectedPrinter;
        private BalanceDTO selectedCartridge;
        private ExpenseDTO selectedExpense;
        private bool searchByPrinter;
        private string cartSearch, printSearch;

        /// <summary>
        /// Представление картриджей.
        /// </summary>
        public ListCollectionView BalanceView
        {
            get { return cartridgeView; }
            private set { cartridgeView = value; RaisePropertyChanged(nameof(BalanceView)); }
        }

        /// <summary>
        /// Представление принтеров.
        /// </summary>
        public ListCollectionView PrintersView
        {
            get { return printersView; }
            private set { printersView = value; RaisePropertyChanged(nameof(PrintersView)); }
        }

        /// <summary>
        /// Новое списание.
        /// </summary>
        public ExpenseDTO NewExpense { get; }

        /// <summary>
        /// Выбранный принтер.
        /// </summary>
        public PrinterDTO SelectedPrinter
        {
            get { return selectedPrinter; }
            set
            {
                selectedPrinter = value;
                RaisePropertyChanged(nameof(SelectedPrinter));
                // Если принтер не выбран, показывать весь баланс, если выбран, показывать совместимые картриджи.
                BalanceView = value is null ? new ListCollectionView(Data.UserData.Balance.Where(x => x.InUse).ToList()) : new ListCollectionView(GetCompatibleBalance());
            }
        }

        /// <summary>
        /// Выбранный картридж.
        /// </summary>
        public BalanceDTO SelectedCartridge
        {
            get { return selectedCartridge; }
            set
            {
                selectedCartridge = value;
                RaisePropertyChanged(nameof(SelectedCartridge));
                if (value != null && NewExpense != null)
                {
                    NewExpense.Cartridge = value.Cartridge;
                }
            }
        }

        /// <summary>
        /// Выбранное списание.
        /// </summary>
        public ExpenseDTO SelectedExpense
        {
            get { return selectedExpense; }
            set { selectedExpense = value; RaisePropertyChanged(nameof(SelectedExpense)); }
        }

        /// <summary>
        /// Стартовая дата для периода.
        /// </summary>
        public DateTime FilterStartDate { get; set; }

        /// <summary>
        /// Конечная дата для периода.
        /// </summary>
        public DateTime FilterEndDate { get; set; }

        /// <summary>
        /// Выбранная опция поиска.
        /// </summary>
        public bool SearchByPrinter
        {
            get { return searchByPrinter; }
            set
            {
                searchByPrinter = value;
                RaisePropertyChanged(nameof(SearchByPrinter));
                // Если отмечен поиск по принтеру и принтер выбран, показывать совместимые картриджи, иначе показывать весь баланс.
                BalanceView = value && SelectedPrinter != null ? new ListCollectionView(GetCompatibleBalance()) : new ListCollectionView(Data.UserData.Balance);
            }
        }

        /// <summary>
        /// Выбор ввода заявки.
        /// </summary>
        public bool BasisChange { get; set; }

        /// <summary>
        /// Строка поиска для картриджей.
        /// </summary>
        public string CartSearch
        {
            get { return cartSearch; }
            set
            {
                cartSearch = value;
                BalanceView.Filter = new Predicate<object>(ViewFilter);
            }
        }

        /// <summary>
        /// Строка поиска для принтеров.
        /// </summary>
        public string PrintSearch
        {
            get { return printSearch; }
            set
            {
                printSearch = value;
                PrintersView.Filter = new Predicate<object>(ViewFilter);
            }
        }

        /// <summary>
        /// Добавить списание.
        /// </summary>
        public ICommand AddExpense
        {
            get
            {
                return new RelayCommand(async obj =>
                {
                    NewExpense.User = Data.UserData.CurrentUser;
                    NewExpense.OspId = Data.UserData.CurrentUser.Osp.Id;
                    // Провести валидацию объекта с получением списка ошибок.
                    List<ValidationResult> results = NewExpense.Validate(new ValidationContext(NewExpense)).ToList();
                    // Если список ошибок не пустой, вывести эти ошибки.
                    if (results.Any())
                    {
                        foreach (var error in results)
                        {
                            Alert.Show(error.ErrorMessage, "Ошибка!", MessageBoxButton.OK);
                        }
                    }
                    // Если ошибок нет.
                    else
                    {
                        // Если выбран номер заявки и он не состоит из 9 цифр.
                        if (BasisChange && !Regex.IsMatch(NewExpense.Basis, @"^[0-9]{9}"))
                        {
                            // Вывести сообщение об ошибке.
                            Alert.Show("Номер заявки должен состоять из 9 цифр.", "Ошибка!", MessageBoxButton.OK);
                            return;
                        }
                        // Иначе продолжить запись.
                        else
                        {
                            // Отправить запрос на сервер о добавлении списания.
                            await ServerConnect.Connection.InvokeAsync("AddExpense", NewExpense);
                            // Очистить строку ввода номера заявки.
                            NewExpense.Basis = string.Empty;
                        }
                    }
                },
                // Доступно если соединение установлено, выбран картридж и остаток положительный.
                (canEx) => ServerConnect.Status && SelectedCartridge != null && SelectedCartridge.Count > 0);
            }
        }

        /// <summary>
        /// Команда обновления списка списаний пользователя за период.
        /// </summary>
        public ICommand UpdateUserExpenses
        {
            get
            {
                return new RelayCommand(async obj =>
                {
                    // Отправить запрос на сервер об обновлении списаний пользователя.
                    await ServerConnect.Connection.InvokeAsync("GetUserExpenses", Data.UserData.CurrentUser.Osp.Id, Data.UserData.CurrentUser.Id, FilterStartDate, FilterEndDate);
                },
                // Доступно если соединение установлено и выбран картридж.
                (canEx) => ServerConnect.Status);
            }
        }

        /// <summary>
        /// Конструктор.
        /// </summary>
        public CreateExpenseVm()
        {
            // Статус отображения окна.
            IsOpened = true;
            // Метка на основании списания - заявка.
            BasisChange = true;
            // Представления принтеров и картриджей.
            BalanceView = new ListCollectionView(Data.UserData.Balance.Where(x => x.InUse).ToList());
            PrintersView = new ListCollectionView(Data.UserData.Printers.Where(x => x.InUse).ToList());
            // Выбранный картридж.
            SelectedCartridge = Data.UserData.Balance.FirstOrDefault();
            // начало и конец периода отображения списаний пользователя.
            FilterStartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day).AddDays(-30);
            FilterEndDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            // Новое списание.
            NewExpense = new ExpenseDTO()
            {
                Basis = string.Empty,
                Date = DateTime.Now.Date,
                Count = 1,
                Delete = false,
                Edit = false,
                User = Data.UserData.CurrentUser,
                Cartridge = SelectedCartridge?.Cartridge,
                OspId = Data.UserData.CurrentUser.Osp.Id
            };

            // Обработчик события обновления всех данных.
            Data.UserData.AllDataUpdated += UserData_UpdateAllData;
            Data.UserData.BalanceUpdated += UserData_BalanceUpdated;
            // Обработчик вызова добавление нового списания пользователя.
            ServerConnect.Connection.On<IEnumerable<ExpenseDTO>>("AddNewUserExpenses", (newUserExpenses) =>
            {
                // Перебрать список новых поступлений пользователя в ОСП.
                foreach (ExpenseDTO userExpense in newUserExpenses)
                {
                    // Если списание не null и дата списания находится внутри диапазона фильтров дат добавить списание.
                    if (userExpense != null && userExpense.Date >= FilterStartDate && userExpense.Date <= FilterEndDate)
                    {
                        Data.UserData.AddNewUserExpense(userExpense);
                    }
                }
                // Выбрать последнее списание.
                SelectedExpense = Data.UserData.UserExpenses.LastOrDefault();
            });
        }


        /// <summary>
        /// Обработчик события обновления баланса.
        /// </summary>
        private void UserData_BalanceUpdated()
        {
            BalanceView = new ListCollectionView(Data.UserData.Balance.Where(x => x.InUse).ToList());
        }

        /// <summary>
        /// Обработчик события обновления всех данных.
        /// </summary>
        private void UserData_UpdateAllData()
        {
            // Представления принтеров и картриджей.
            BalanceView = new ListCollectionView(Data.UserData.Balance.Where(x => x.InUse).ToList());
            PrintersView = new ListCollectionView(Data.UserData.Printers.Where(x => x.InUse).ToList());
        }

        /// <summary>
        /// Получить совместимые картриджи выбранного принтера в балансе.
        /// </summary>
        /// <returns>Совместимые картриджи в балансе</returns>
        private ObservableCollection<BalanceDTO> GetCompatibleBalance()
        {
            // Получить массив Id совместимых с выбранным принтером картриджей.
            IEnumerable<int> compCartId = SelectedPrinter.Compatibility.Select(x => x.Id);
            // Получить картриджи из баланса по Id совместимых картриджей.
            IEnumerable<BalanceDTO> compBalance = Data.UserData.Balance.Where(x => x.InUse && compCartId.Contains(x.Cartridge.Id));
            // Вернуть коллекцию картриджей баланса.
            return new ObservableCollection<BalanceDTO>(compBalance);
        }

        /// <summary>
        /// Фильтр представлений списков принтеров и картриджей.
        /// </summary>
        /// <param name="obj">Объект фильтрации</param>
        /// <returns>Проходит ли объект фильтрацию</returns>
        private bool ViewFilter(object obj)
        {
            if (obj is PrinterDTO print)
            {
                return string.IsNullOrWhiteSpace(PrintSearch)
                    || (SelectedPrinter != null && SelectedPrinter.Model == PrintSearch)
                    || print.Model.ToUpper().Contains(PrintSearch.ToUpper());
            }
            else if (obj is BalanceDTO balance)
            {
                return string.IsNullOrWhiteSpace(CartSearch)
                    || (SelectedCartridge != null && SelectedCartridge.Cartridge.Model == CartSearch)
                    || balance.Cartridge.Model.ToUpper().Contains(CartSearch.ToUpper());
            }
            else
            {
                return false;
            }
        }
    }
}
