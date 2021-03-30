using CartAccClient.Model;
using CartAccClient.View;
using CartAccLibrary.Comparers;
using CartAccLibrary.Dto;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace CartAccClient.ViewModel
{
    /// <summary>
    /// ViewModel вкладки списаний.
    /// </summary>
    class ExpensesVm : MainViewModel
    {
        private ListCollectionView expensesView, balanceView;
        private ExpenseDTO selectedExpense;
        private UserDTO selectedAutor;
        private CartridgeDTO selectedCartridge;
        private string searchString;


        /// <summary>
        /// Представление коллекции списаний.
        /// </summary>
        public ListCollectionView ExpensesView
        {
            get { return expensesView; }
            private set { expensesView = value; RaisePropertyChanged(nameof(ExpensesView)); }
        }

        /// <summary>
        /// Представление коллекции баланса.
        /// </summary>
        public ListCollectionView BalanceView
        {
            get { return balanceView; }
            private set { balanceView = value; RaisePropertyChanged(nameof(BalanceView)); }
        }

        /// <summary>
        /// Выбранное списание.
        /// </summary>
        public ExpenseDTO SelectedExpense
        {
            get { return selectedExpense; }
            set
            {
                CheckUnsaved();
                selectedExpense = value;
                CartridgeOfSelectedExpense = value is null ? null : Data.UserData.Balance.FirstOrDefault(x => x.Cartridge.Id == value.Cartridge.Id);
                BalanceView = new ListCollectionView(Data.UserData.Balance.Where(x => x.InUse).ToList());
                RaisePropertyChanged(nameof(SelectedExpense));
            }
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
        /// Коллекция авторов для фильтра.
        /// </summary>
        public ObservableCollection<UserDTO> AutorsFilter { get; }

        /// <summary>
        /// Коллекция картриджей для фильтра.
        /// </summary>
        public ObservableCollection<CartridgeDTO> CartridgesFilter { get; }

        /// <summary>
        /// Результаты поиска.
        /// </summary>
        public List<ExpenseDTO> SearchResult { get; private set; }

        /// <summary>
        /// Выбранный в фильтре автор.
        /// </summary>
        public UserDTO SelectedFilterAutor
        {
            get { return selectedAutor; }
            set { selectedAutor = value; RaisePropertyChanged(nameof(SelectedFilterAutor)); }
        }

        /// <summary>
        /// Выбранный в фильтре картридж.
        /// </summary>
        public CartridgeDTO SelectedFilterCartridge
        {
            get { return selectedCartridge; }
            set { selectedCartridge = value; RaisePropertyChanged(nameof(SelectedFilterCartridge)); }
        }

        /// <summary>
        /// Картридж выбранного списания.
        /// </summary>
        public BalanceDTO CartridgeOfSelectedExpense
        {
            get
            {
                return SelectedExpense is null ? null : Data.UserData.Balance.FirstOrDefault(x => x.Cartridge.Id == SelectedExpense.Cartridge.Id);
            }
            set
            {
                // Если списание выбранои значение не null.
                if (SelectedExpense != null && value != null)
                {
                    // Установить в списании выбранный при редактировании картридж.
                    SelectedExpense.Cartridge = value.Cartridge;
                }
                RaisePropertyChanged(nameof(CartridgeOfSelectedExpense));
            }
        }

        /// <summary>
        /// Строка поискового запроса.
        /// </summary>
        public string SearchString
        {
            get { return searchString; }
            set { searchString = value; RaisePropertyChanged(nameof(SearchString)); }
        }

        /// <summary>
        /// Изменить выбранный документ.
        /// </summary>
        public ICommand StartEdit
        {
            get
            {
                return new RelayCommand(async obj =>
                {
                    // Отправить запрос на открытие документа для изменения.
                    await ServerConnect.Connection.InvokeAsync("OpenDocument",
                        SelectedExpense.Id, Data.UserData.CurrentUser.Osp.Id, SelectedExpense.GetType().Name, Data.UserData.CurrentUser.Fullname);
                },
                // Доступно если соединение установлено, выбрано списание и уровень доступа 3 или меньше (менеджер).
                (canEx) => ServerConnect.Status && SelectedExpense != null && Data.UserData.CurrentUser.Access.Id <= 3);
            }
        }

        /// <summary>
        /// Сохранить изменения.
        /// </summary>
        public ICommand AcceptChanges
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    // Проверить и отправить списание на сохранение в бд.
                    ValidateAndSendExpenseAsync();
                },
                // Команда доступна если есть подключение, включен режим редактирования и списание не null.
                (canEx) => ServerConnect.Status && CanEdit && SelectedExpense != null);
            }
        }

        /// <summary>
        /// Сбросить изменения.
        /// </summary>
        public ICommand DiscardChanges
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    DiscardChangesAsync();
                },
                // Команда доступна если есть подключение и включен режим редактирования.
                (canEx) => ServerConnect.Status && CanEdit);
            }
        }

        /// <summary>
        /// Применить фильтры.
        /// </summary>
        public ICommand ApplyFilter
        {
            get
            {
                return new RelayCommand(async obj =>
                {
                    // Обновить списания за период.
                    await ServerConnect.Connection.InvokeAsync("GetOspExpenses", Data.UserData.CurrentUser.Osp.Id, FilterStartDate, FilterEndDate);
                    ExpensesView.Filter = new Predicate<object>(Filtering);
                },
                // Команда доступна если есть подключение и выбраны автор и картридж.
                (canEx) => ServerConnect.Status && SelectedFilterAutor != null && SelectedFilterCartridge != null);
            }
        }

        /// <summary>
        /// Сбросить фильтры.
        /// </summary>
        public ICommand CancelFilter
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    // Сбросить фильтрацию.
                    ExpensesView.Filter = null;
                    // Выбрать первые элементы фильтров.
                    SelectedFilterAutor = AutorsFilter.First();
                    SelectedFilterCartridge = CartridgesFilter.First();
                });
            }
        }

        /// <summary>
        /// Найти.
        /// </summary>
        public ICommand Search
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    // Парсим строку поиска в число.
                    int.TryParse(SearchString, out int number);
                    // Выбрать из списаний те, у которых поисковой строке соответствует номер или заявка.
                    SearchResult = Data.UserData.Expenses.Where(x => x.Number == number || x.Basis == SearchString).ToList();
                    // Если результат не пустой.
                    if (SearchResult.Any())
                    {
                        // Установить выбранным списанием первый результат поиска.
                        SelectedExpense = SearchResult.FirstOrDefault();
                    }
                    // Иначе вывести сообщение.
                    else
                    {
                        Alert.Show("Не найдено.", "Поиск списания", MessageBoxButton.OK);
                    }
                });
            }
        }

        /// <summary>
        /// Найти далее.
        /// </summary>
        public ICommand SearchNext
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    // Определить индекс выбранного списания в коллекции результатов поиска.
                    int index = SearchResult.FindIndex(x => x.Id == SelectedExpense.Id);
                    // Если списание не последнее в списке.
                    if (SelectedExpense != SearchResult.Last())
                    {
                        // Установить выбранным списанием следующий результат поиска.
                        SelectedExpense = SearchResult[index + 1];
                    }
                    // Иначе вывести сообщение.
                    else
                    {
                        Alert.Show("Поиск завершен", "Поиск", MessageBoxButton.OK);
                    }
                },
                // Доступно когда результатов поиска больше 1.
                (canEx) => SearchResult.Count > 1);
            }
        }

        /// <summary>
        /// Пустой конструктор.
        /// </summary>
        public ExpensesVm()
        {
            // Инициализировать фильтры.
            AutorsFilter = new ObservableCollection<UserDTO>();
            CartridgesFilter = new ObservableCollection<CartridgeDTO>();
            // Выбрать первые значения.
            SelectedFilterAutor = AutorsFilter.FirstOrDefault();
            SelectedFilterCartridge = CartridgesFilter.FirstOrDefault();
            // Обновить фильтры.
            UpdateFilters();
            // Инициализировать список результатов поиска.
            SearchResult = new List<ExpenseDTO>();
            // Инициализировать представление списаний.
            ExpensesView = new ListCollectionView(Data.UserData.Expenses);
            // Даты для фильтра, последние 30 дней.
            FilterStartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day).AddDays(-30);
            FilterEndDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);

            // Обработчик события обновления всех данных.
            Data.UserData.AllDataUpdated += UserData_UpdateAllData;

            // Обработчик вызова добавление новых списаний в ОСП.
            ServerConnect.Connection.On<IEnumerable<ExpenseDTO>>("AddNewOspExpenses", (newOspExpenses) =>
            {
                // Перебрать список новых списаний в ОСП.
                foreach (ExpenseDTO ospExpense in newOspExpenses)
                {
                    // Если списание не null и дата списания находится внутри диапазона фильтров дат добавить списание.
                    if (ospExpense != null && ospExpense.Date >= FilterStartDate && ospExpense.Date <= FilterEndDate)
                    {
                        Data.UserData.AddNewOspExpense(ospExpense);
                    }
                }
            });
            // Обработчик вызова разрешения редактирования документа списания.
            ServerConnect.Connection.On("AllowExpenseEdit", () => CanEdit = true);
        }


        /// <summary>
        /// Обработчик события обновления всех данных.
        /// </summary>
        private void UserData_UpdateAllData()
        {
            ExpensesView = new ListCollectionView(Data.UserData.Expenses)
            {
                // Применить фильтрацию.
                Filter = new Predicate<object>(Filtering)
            };
            // Обновить фильтры.
            UpdateFilters();
        }

        /// <summary>
        /// Метод фильтрации.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private bool Filtering(object obj)
        {
            // Если объекты не null.
            if (obj is ExpenseDTO item && SelectedFilterAutor != null && SelectedFilterCartridge != null)
            {
                // Если выбран конкретный картридж.
                if (SelectedFilterAutor.Id == 0 && SelectedFilterCartridge.Id != 0)
                {
                    // Вернуть объекты, у которых Id картриджа равен Id выбранного в фильтре картриджа.
                    return item.Cartridge.Id == SelectedFilterCartridge.Id;
                }
                // Если выбран конкретный автор.
                else if (SelectedFilterCartridge.Id == 0 && SelectedFilterAutor.Id != 0)
                {
                    // Вернуть объекты, у которых Id автора равен Id выбранного автора.
                    return item.User.Id == SelectedFilterAutor.Id;
                }
                // Если конкретные фильтры не выбраны.
                else if (SelectedFilterAutor.Id == 0 && SelectedFilterCartridge.Id == 0)
                {
                    // Вернуть все объекты.
                    return true;
                }
                // Если выбраны конкретные автор и картридж.
                else
                {
                    // Вернуть объекты, у которых Id автора равен Id выбранного автора и Id картриджа равен Id выбранного в фильтре картриджа.
                    return item.User.Id == SelectedFilterAutor.Id && item.Cartridge.Id == SelectedFilterCartridge.Id;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Обновляет фильтры списаний.
        /// </summary>
        private void UpdateFilters()
        {
            // Запомнить Id выбранных элементов фильтров.
            int selectedAutorId = SelectedFilterAutor is null ? 0 : SelectedFilterAutor.Id;
            int selectedCartridgeId = SelectedFilterCartridge is null ? 0 : SelectedFilterCartridge.Id;
            // Очистить списки фильтров.
            AutorsFilter.Clear();
            CartridgesFilter.Clear();
            // Вставить на первую позицию вариант всех значений.
            AutorsFilter.Insert(0, new UserDTO(0, "Все", "Все", null, null));
            CartridgesFilter.Insert(0, new CartridgeDTO(0, "Все", null));
            // Выбрать новые значения.
            var autors = Data.UserData.Expenses.Select(x => x.User).Distinct(new UserDTOComparer()).ToList();
            var carts = Data.UserData.Expenses.Select(x => x.Cartridge).Distinct(new CartridgeDTOComparer()).ToList();
            // Добавить новые значения в списки фильтров.
            foreach (var autor in autors)
            {
                AutorsFilter.Add(autor);
            }
            foreach (var cart in carts)
            {
                CartridgesFilter.Add(cart);
            }
            // Установить выбранные варианты фильтров по сохраненным ранее Id.
            SelectedFilterAutor = AutorsFilter.FirstOrDefault(x => x.Id == selectedAutorId);
            SelectedFilterCartridge = CartridgesFilter.FirstOrDefault(x => x.Id == selectedCartridgeId);
        }

        /// <summary>
        /// Проверка на несохраненные данные при изменении выбранного списания.
        /// </summary>
        private void CheckUnsaved()
        {
            // Если режим редактирования активен.
            if (CanEdit)
            {
                // Спросить у пользователя о действии при несохраненных данных.
                MessageBoxResult resultDialog = Alert.Show("Изменения не сохранены.\nСохранить?", "Редактирование", MessageBoxButton.YesNo);
                // В зависимости от результата диалога.
                switch (resultDialog)
                {
                    case MessageBoxResult.Yes:
                        ValidateAndSendExpenseAsync();
                        break;
                    case MessageBoxResult.No:
                        DiscardChangesAsync();
                        break;
                }
            }
        }

        /// <summary>
        /// Проводит проверку списания и отправку на сервер.
        /// </summary>
        private async void ValidateAndSendExpenseAsync()
        {
            // Провести валидацию объекта с получением списка ошибок.
            List<ValidationResult> results = SelectedExpense.Validate(new ValidationContext(SelectedExpense)).ToList();
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
                // Отправить списание на сохранение в бд
                await ServerConnect.Connection.InvokeAsync("UpdateExpense", SelectedExpense);
                // Закрыть режим редактирования.
                CanEdit = false;
            }
        }

        /// <summary>
        /// Отменяет редактирование списания.
        /// </summary>
        private async void DiscardChangesAsync()
        {
            // Отменить режим редактирования.
            CanEdit = false;
            // Если списание Новое.
            if (SelectedExpense.Id == 0)
            {
                // Сбросить выбранное поступление.
                SelectedExpense = null;
            }
            else
            {
                // Отправить запрос на закрытие документа.
                await ServerConnect.Connection.InvokeAsync("CloseDocument", SelectedExpense.Id, SelectedExpense.GetType().Name, Data.UserData.CurrentUser.Osp.Id);
                // Отправить запрос на получение списания.
                await ServerConnect.Connection.InvokeAsync("GetExpense", SelectedExpense.Id);
            }
        }
    }
}
