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
    /// ViewModel вкладки поступлений.
    /// </summary>
    class ReceiptsVm : MainViewModel
    {
        private ListCollectionView receiptsView, receiptCartridgesView, providersView;
        private ReceiptDTO selectedReceipt;
        private UserDTO selectedAutor;
        private ProviderDTO selectedProvider;
        private string searchString, cartridgeSearch;
        private bool showZeroCount;


        /// <summary>
        /// Представление коллекции поступлений.
        /// </summary>
        public ListCollectionView ReceiptsView
        {
            get { return receiptsView; }
            set { receiptsView = value; RaisePropertyChanged(nameof(ReceiptsView)); }
        }

        /// <summary>
        /// Представление коллекции картриджей поступления.
        /// </summary>
        public ListCollectionView ReceiptCartridgesView
        {
            get { return receiptCartridgesView; }
            set { receiptCartridgesView = value; RaisePropertyChanged(nameof(ReceiptCartridgesView)); }
        }

        /// <summary>
        /// Представление коллекции поставщиков для поступления.
        /// </summary>
        public ListCollectionView ProvidersView
        {
            get { return providersView; }
            set { providersView = value; RaisePropertyChanged(nameof(ProvidersView)); }
        }

        /// <summary>
        /// Выбранное поступление.
        /// </summary>
        public ReceiptDTO SelectedReceipt
        {
            get { return selectedReceipt; }
            set
            {
                // Проверка на сохранение данных поступления при редактировании.
                CheckUnsaved();
                selectedReceipt = value;
                // Установить выбранного поставщика.
                ProviderOfSelectedExpense = value is null
                    ? null
                    : SelectedReceipt.Provider is null
                        ? null
                        : Data.UserData.Providers.FirstOrDefault(x => x.Id == SelectedReceipt.Provider.Id);
                // Установить представление картриджей поступления.
                ReceiptCartridgesView = new ListCollectionView(selectedReceipt is null ? new List<ReceiptCartridgeDTO>() : selectedReceipt.Cartridges.ToList());
                // Установить представление поставщиков (только актуальных).
                ProvidersView = new ListCollectionView(Data.UserData.Providers.Where(x => x.Active).ToList());
                RaisePropertyChanged(nameof(SelectedReceipt));
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
        public ObservableCollection<ProviderDTO> ProvidersFilter { get; }

        /// <summary>
        /// Результаты поиска.
        /// </summary>
        public List<ReceiptDTO> SearchResult { get; private set; }

        /// <summary>
        /// Выбранный в фильтре автор.
        /// </summary>
        public UserDTO SelectedFilterAutor
        {
            get { return selectedAutor; }
            set { selectedAutor = value; RaisePropertyChanged(nameof(SelectedFilterAutor)); }
        }

        /// <summary>
        /// Выбранный в фильтре поставщик.
        /// </summary>
        public ProviderDTO SelectedFilterProvider
        {
            get { return selectedProvider; }
            set { selectedProvider = value; RaisePropertyChanged(nameof(SelectedFilterProvider)); }
        }

        /// <summary>
        /// Поставщик выбранного поступления.
        /// </summary>
        public ProviderDTO ProviderOfSelectedExpense
        {
            get
            {
                return SelectedReceipt is null
                    ? null
                    : SelectedReceipt.Provider is null
                        ? null
                        : Data.UserData.Providers.FirstOrDefault(x => x.Id == SelectedReceipt.Provider.Id);
            }
            set
            {
                if (SelectedReceipt != null)
                {
                    SelectedReceipt.Provider = value;
                }
                RaisePropertyChanged(nameof(ProviderOfSelectedExpense));
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
        /// Строка поискового запроса для картриджа.
        /// </summary>
        public string CartridgeSearch
        {
            get { return cartridgeSearch; }
            set
            {
                cartridgeSearch = value;
                RaisePropertyChanged(nameof(CartridgeSearch));
                if (ReceiptCartridgesView != null)
                {
                    ReceiptCartridgesView.Filter = new Predicate<object>(ReceiptCartridgeModelFiltering);
                }
            }
        }

        /// <summary>
        /// Отображение картриджей поступления с нулевым количеством.
        /// </summary>
        public bool ShowZeroCount
        {
            get { return showZeroCount; }
            set
            {
                showZeroCount = value;
                RaisePropertyChanged(nameof(ShowZeroCount));
                if (ReceiptCartridgesView != null)
                {
                    ReceiptCartridgesView.Filter = new Predicate<object>(ReceiptCartridgeModelFiltering);
                }
            }
        }

        /// <summary>
        /// Создать новое поступление.
        /// </summary>
        public ICommand MakeNewReceipt
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    // Создать список картриджей для поступлений из словаря картриджей (только используемые).
                    List<ReceiptCartridgeDTO> allCartsForReceipt = Data.UserData.Cartridges.Where(x => x.InUse).Select(x => new ReceiptCartridgeDTO(0, x, 0)).ToList();
                    // Создать новый объект списания.
                    SelectedReceipt = new ReceiptDTO()
                    {
                        Date = DateTime.Now.Date,
                        Comment = string.Empty,
                        User = Data.UserData.CurrentUser,
                        Provider = new ProviderDTO(),
                        Cartridges = new ObservableCollection<ReceiptCartridgeDTO>(allCartsForReceipt),
                        Delete = false,
                        Edit = false,
                        OspId = Data.UserData.CurrentUser.Osp.Id
                    };
                    // Открыть режим редактирования.
                    CanEdit = true;
                },
                // Доступно если соединение установлено и уровень доступа 3 или меньше (менеджер).
                (canEx) => ServerConnect.Status && Data.UserData.CurrentUser.Access.Id <= 3);
            }
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
                        SelectedReceipt.Id, Data.UserData.CurrentUser.Osp.Id, SelectedReceipt.GetType().Name, Data.UserData.CurrentUser.Fullname);
                },
                // Доступно если соединение установлено, выбрано поступление и уровень доступа 3 или меньше (менеджер).
                (canEx) => ServerConnect.Status && SelectedReceipt != null && Data.UserData.CurrentUser.Access.Id <= 3);
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
                    // Проверка на выбранного поставщика.
                    if (SelectedReceipt.Provider is null && !SelectedReceipt.Comment.Contains("Коррекция"))
                    {
                        Alert.Show("Выберите поставщика.");
                    }
                    else
                    {
                        ValidateAndSendReceiptAsync();
                    }
                },
                // Команда доступна если есть подключение, включен режим редактирования и выбраано поступление.
                (canEx) => ServerConnect.Status && CanEdit && SelectedReceipt != null);
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
                    // Отменить редактирование и изменения.
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
                    // Обновить поступления за период.
                    await ServerConnect.Connection.InvokeAsync("GetOspReceipts", Data.UserData.CurrentUser.Osp.Id, FilterStartDate, FilterEndDate);
                    ReceiptsView.Filter = new Predicate<object>(ReceiptFiltering);
                },
                // Команда доступна если есть подключение и выбраны автор и поставщик.
                (canEx) => ServerConnect.Status && SelectedFilterAutor != null && SelectedFilterProvider != null);
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
                    ReceiptsView.Filter = null;
                    // Выбрать первые элементы фильтров.
                    SelectedFilterAutor = AutorsFilter.First();
                    SelectedFilterProvider = ProvidersFilter.First();
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
                    // Выбрать из поступлений те, у которых поисковой строке соответствует номер.
                    SearchResult = Data.UserData.Receipts.Where(x => x.Number == number).ToList();
                    // Если результат не пустой.
                    if (SearchResult.Any())
                    {
                        // Установить выбранным поступлением первый результат поиска.
                        SelectedReceipt = SearchResult.FirstOrDefault();
                    }
                    // Иначе вывести сообщение.
                    else
                    {
                        Alert.Show("Не найдено.", "Поиск поступления", MessageBoxButton.OK);
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
                    // Определить индекс выбранного поступления в коллекции результатов поиска.
                    int index = SearchResult.FindIndex(x => x.Id == SelectedReceipt.Id);
                    // Если поступление не последнее в списке.
                    if (SelectedReceipt != SearchResult.Last())
                    {
                        // Установить выбранным поступлением следующий результат поиска.
                        SelectedReceipt = SearchResult[index + 1];
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
        /// Конструктор.
        /// </summary>
        public ReceiptsVm()
        {
            // Инициализировать фильтры.
            AutorsFilter = new ObservableCollection<UserDTO>();
            ProvidersFilter = new ObservableCollection<ProviderDTO>();
            // Выбрать первые значения.
            SelectedFilterAutor = AutorsFilter.FirstOrDefault();
            SelectedFilterProvider = ProvidersFilter.FirstOrDefault();
            // Обновить фильтры.
            UpdateFilters();
            // Инициализировать список результатов поиска.
            SearchResult = new List<ReceiptDTO>();
            // Инициализировать представление поступлений.
            ReceiptsView = new ListCollectionView(Data.UserData.Receipts);
            // Даты для фильтра, последние 30 дней.
            FilterStartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day).AddDays(-30);
            FilterEndDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            // Строка поиска картриджей поступления.
            CartridgeSearch = string.Empty;
            // Отображать картриджи с 0 количеством.
            ShowZeroCount = true;

            // Обработчик события обновления всех данных.
            Data.UserData.AllDataUpdated += UserData_UpdateAllData;

            // Обработчик вызова добавления нового поступления в ОСП.
            ServerConnect.Connection.On<ReceiptDTO>("AddNewOspReceipt", (newOspReceipt) =>
            {
                // Если поступление не null и дата поступления находится внутри диапазона фильтров дат добавить поступление.
                if (newOspReceipt != null && newOspReceipt.Date >= FilterStartDate && newOspReceipt.Date <= FilterEndDate)
                {
                    Data.UserData.AddNewOspReceipt(newOspReceipt);
                }
            });
            // Обработчик вызова разрешения редактирования документа поступления.
            ServerConnect.Connection.On("AllowReceiptEdit", () =>
            {
                // Создать список картриджей для поступлений из словаря картриджей (только используемые).
                List<ReceiptCartridgeDTO> allCartsForReceipt = Data.UserData.Cartridges
                    .Where(x => x.InUse)
                    .Select(x => new ReceiptCartridgeDTO(0, x, 0))
                    .ToList();
                // Добавить в список картриджей те картриджи, которые не содержатся в списке поступления.
                SelectedReceipt.Cartridges = new ObservableCollection<ReceiptCartridgeDTO>(
                        SelectedReceipt.Cartridges.Union(allCartsForReceipt, new ReceiptCartridgeDTOComparer())
                    );
                // Установить представление картриджей поступления.
                ReceiptCartridgesView = new ListCollectionView(SelectedReceipt is null ? new List<ReceiptCartridgeDTO>() : SelectedReceipt.Cartridges.ToList());
                // Включить режим редактирования.
                CanEdit = true;
                // Включить отображение картриджей с нулевым остатком.
                ShowZeroCount = true;
            });
        }


        /// <summary>
        /// Обработчик события обновления всех данных.
        /// </summary>
        private void UserData_UpdateAllData()
        {
            // Инициализировать представление поступлений.
            ReceiptsView = new ListCollectionView(Data.UserData.Receipts)
            {
                // Применить фильтрацию.
                Filter = new Predicate<object>(ReceiptFiltering)
            };
            // Обновить фильтры.
            UpdateFilters();
        }

        /// <summary>
        /// Фильтрация поступлений.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private bool ReceiptFiltering(object obj)
        {
            // Если объекты не null.
            if (obj is ReceiptDTO item && SelectedFilterAutor != null && SelectedFilterProvider != null)
            {
                // Если выбран конкретный поставщик.
                if (SelectedFilterAutor.Id == 0 && SelectedFilterProvider.Id != 0)
                {
                    // Вернуть объекты, у которых Id поставщика равен Id выбранного в фильтре поставщика.
                    return item.Provider.Id == SelectedFilterProvider.Id;
                }
                // Если выбран конкретный автор.
                else if (SelectedFilterProvider.Id == 0 && SelectedFilterAutor.Id != 0)
                {
                    // Вернуть объекты, у которых Id автора равен Id выбранного автора.
                    return item.User.Id == SelectedFilterAutor.Id;
                }
                // Если конкретные фильтры не выбраны.
                else if (SelectedFilterAutor.Id == 0 && SelectedFilterProvider.Id == 0)
                {
                    // Вернуть все объекты.
                    return true;
                }
                // Если выбраны конкретные автор и поставщик.
                else
                {
                    // Вернуть объекты, у которых Id автора равен Id выбранного автора и Id поставщика равен Id выбранного в фильтре поставщика.
                    return item.User.Id == SelectedFilterAutor.Id && item.Provider.Id == SelectedFilterProvider.Id;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Фильтрация картриджей поступления по модели.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private bool ReceiptCartridgeModelFiltering(object obj)
        {
            // Если фильтруемый объект приводится к картриджу поступления.
            if (obj is ReceiptCartridgeDTO item)
            {
                if (ShowZeroCount)
                {
                    return item.Cartridge.Model.ToUpper().Contains(CartridgeSearch.ToUpper());
                }
                else
                {
                    return item.Cartridge.Model.ToUpper().Contains(CartridgeSearch.ToUpper()) && item.Count > 0;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Обновляет фильтры поступлений.
        /// </summary>
        private void UpdateFilters()
        {
            // Запомнить Id выбранных элементов фильтров.
            int selectedAutorId = SelectedFilterAutor is null ? 0 : SelectedFilterAutor.Id;
            int selectedProviderId = SelectedFilterProvider is null ? 0 : SelectedFilterProvider.Id;
            // Очистить списки фильтров.
            AutorsFilter.Clear();
            ProvidersFilter.Clear();
            // Вставить на первую позицию вариант всех значений.
            AutorsFilter.Insert(0, new UserDTO(0, "Все", "Все", null, null));
            ProvidersFilter.Insert(0, new ProviderDTO(0, 0, "Все", string.Empty, 0, true));
            // Выбрать новые значения.
            var autors = Data.UserData.Receipts.Select(x => x.User).Distinct(new UserDTOComparer()).ToList();
            var providers = Data.UserData.Receipts.Select(x => x.Provider).Distinct(new ProviderDTOComparer()).ToList();
            // Добавить новые значения в списки фильтров.
            foreach (var autor in autors)
            {
                AutorsFilter.Add(autor);
            }
            foreach (var provider in providers)
            {
                ProvidersFilter.Add(provider);
            }
            // Установить выбранные варианты фильтров по сохраненным ранее Id.
            SelectedFilterAutor = AutorsFilter.FirstOrDefault(x => x.Id == selectedAutorId);
            SelectedFilterProvider = ProvidersFilter.FirstOrDefault(x => x.Id == selectedProviderId);
        }

        /// <summary>
        /// Проверка на несохраненные данные при изменении выбранного поступления.
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
                        ValidateAndSendReceiptAsync();
                        break;
                    case MessageBoxResult.No:
                        DiscardChangesAsync();
                        break;
                }
            }
        }

        /// <summary>
        /// Проводит проверку поступления и отправку на сервер.
        /// </summary>
        private async void ValidateAndSendReceiptAsync()
        {
            // Преобразовать в список коллекцию картриджей в редактируемом поступлении.
            var receiptCarts = SelectedReceipt.Cartridges.Where(x => x.Count > 0).ToList();
            // Присвоить измененный список картриджей поступления.
            SelectedReceipt.Cartridges = new ObservableCollection<ReceiptCartridgeDTO>(receiptCarts);
            // Провести валидацию объекта с получением списка ошибок.
            List<ValidationResult> results = SelectedReceipt.Validate(new ValidationContext(SelectedReceipt)).ToList();
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
                // Если поступление новое.
                if (SelectedReceipt.Id == 0)
                {
                    // Отправить поступление на сохранение в бд
                    await ServerConnect.Connection.InvokeAsync("AddReceipt", SelectedReceipt);
                }
                // Если поступление существующее.
                else
                {
                    // Отправить поступление на сохранение в бд
                    await ServerConnect.Connection.InvokeAsync("UpdateReceipt", SelectedReceipt);
                }
                // Закрыть режим редактирования.
                CanEdit = false;
                // Сбросить выбранное поступление.
                SelectedReceipt = null;
            }
        }

        /// <summary>
        /// Отменяет редактирование поступления.
        /// </summary>
        private async void DiscardChangesAsync()
        {
            // Отменить режим редактирования.
            CanEdit = false;
            // Очистить строку поиска картриджа.
            CartridgeSearch = string.Empty;
            // Если поступление новое.
            if (SelectedReceipt.Id == 0)
            {
                // Отменить режим редактирования.
                CanEdit = false;
                // Сбросить выбранное поступление.
                SelectedReceipt = null;
            }
            else
            {
                // Отправить запрос на закрытие документа.
                await ServerConnect.Connection.InvokeAsync("CloseDocument", SelectedReceipt.Id, SelectedReceipt.GetType().Name, Data.UserData.CurrentUser.Osp.Id);
                // Отправить запрос на получение поступления.
                await ServerConnect.Connection.InvokeAsync("GetReceipt", SelectedReceipt.Id);
            }
        }
    }
}
