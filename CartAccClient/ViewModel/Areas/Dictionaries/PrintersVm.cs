using CartAccClient.Model;
using CartAccClient.View;
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
    /// Viewmodel вкладки словаря принтеров.
    /// </summary>
    class PrintersVm : MainViewModel
    {
        private ListCollectionView printersView;
        private ListCollectionView addedPrintersView;
        private ListCollectionView compCartridgesView;
        private PrinterDTO selectedPrinter;
        private CartridgeDTO selectedCompCartridge;
        private CartridgeDTO selectedAddedCartridge;
        private string searchString;
        private string cartridgeSearch;

        /// <summary>
        /// Представление коллекции принтеров.
        /// </summary>
        public ListCollectionView PrintersView
        {
            get { return printersView; }
            set { printersView = value; RaisePropertyChanged(nameof(PrintersView)); }
        }

        /// <summary>
        /// Представление коллекции добавляемых совместимых картриджей.
        /// </summary>
        public ListCollectionView AddedCartridgesView
        {
            get { return addedPrintersView; }
            set { addedPrintersView = value; RaisePropertyChanged(nameof(AddedCartridgesView)); }
        }

        /// <summary>
        /// Представление коллекции совместимых картриджей.
        /// </summary>
        public ListCollectionView CompCartridgesView
        {
            get { return compCartridgesView; }
            set { compCartridgesView = value; RaisePropertyChanged(nameof(CompCartridgesView)); }
        }

        /// <summary>
        /// Выбранный принтер.
        /// </summary>
        public PrinterDTO SelectedPrinter
        {
            get { return selectedPrinter; }
            set
            {
                CheckUnsaved();
                selectedPrinter = value;
                RaisePropertyChanged(nameof(SelectedPrinter));
                if (selectedPrinter != null)
                {
                    UpdateCartridgesViews();
                }
                SelectedCompCartridge = null;
                CartridgeSearch = string.Empty;
            }
        }

        /// <summary>
        /// Выбранный совместимый картридж.
        /// </summary>
        public CartridgeDTO SelectedCompCartridge
        {
            get { return selectedCompCartridge; }
            set { selectedCompCartridge = value; RaisePropertyChanged(nameof(SelectedCompCartridge)); }
        }

        /// <summary>
        /// Выбранный добавляемый совместимый картридж.
        /// </summary>
        public CartridgeDTO SelectedAddedCompCartridge
        {
            get { return selectedAddedCartridge; }
            set { selectedAddedCartridge = value; RaisePropertyChanged(nameof(SelectedAddedCompCartridge)); }
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
                PrintersView.Filter = new Predicate<object>(PrinterFiltering);
            }
        }

        /// <summary>
        /// Строка поиска картриджа.
        /// </summary>
        public string CartridgeSearch
        {
            get { return cartridgeSearch; }
            set
            {
                cartridgeSearch = value;
                RaisePropertyChanged(nameof(CartridgeSearch));
                AddedCartridgesView.Filter = new Predicate<object>(CartridgeFiltering);
            }
        }

        /// <summary>
        /// Создать новый принтер.
        /// </summary>
        public ICommand MakeNewPrinter
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    // Создать новый объект картриджа.
                    SelectedPrinter = new PrinterDTO()
                    {
                        Id = 0,
                        Model = string.Empty,
                        Compatibility = new ObservableCollection<CartridgeDTO>(),
                        InUse = true
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
                        SelectedPrinter.Id, 0, SelectedPrinter.GetType().Name, Data.UserData.CurrentUser.Fullname);
                },
                // Доступно если соединение установлено, выбран картридж и уровень доступа 3 или меньше (менеджер).
                (canEx) => ServerConnect.Status && SelectedPrinter != null && Data.UserData.CurrentUser.Access.Id <= 3);
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
                    // Проверка на повтор.
                    if (SelectedPrinter.Id == 0 && Data.UserData.Printers.Any(x => x.Model == SelectedPrinter.Model))
                    {
                        Alert.Show("Принтер уже существует!");
                    }
                    else
                    {
                        ValidateAndSendReceiptAsync();
                    }
                },
                // Команда доступна если есть подключение, включен режим редактирования и выбран картридж.
                (canEx) => ServerConnect.Status && CanEdit && SelectedPrinter != null);
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
        /// Добавить совместимый картридж.
        /// </summary>
        public ICommand AddCompCartridge
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    // Добавить выбранный принтер в список совместимых.
                    SelectedPrinter.Compatibility.Add(SelectedAddedCompCartridge);
                    // Обновить представления принтеров.
                    UpdateCartridgesViews();
                },
                // Команда доступна если выбран добавляемый принтер и включен режим редактирования.
                (canEx) => SelectedAddedCompCartridge != null && CanEdit);
            }
        }

        /// <summary>
        /// Удалить совместимый картридж.
        /// </summary>
        public ICommand RemoveCompCartridge => new RelayCommand(obj =>
                                                             {
                    // Удалить выбранный совместимый принтер из списка.
                    SelectedPrinter.Compatibility.Remove(SelectedCompCartridge);
                    // Обновить представления принтеров.
                    UpdateCartridgesViews();
                                                             },
                // Команда доступна если выбран совместимый принтер в списке и включен режим редактирования.
                (canEx) => SelectedCompCartridge != null && CanEdit);

        /// <summary>
        /// Конструктор.
        /// </summary>
        public PrintersVm()
        {
            // Инициализировать представление принтеров.
            PrintersView = new ListCollectionView(Data.UserData.Printers);
            // Обработчик события обновления всех данных.
            Data.UserData.AllDataUpdated += UserData_UpdateAllData;
            // Обработчик вызова добавления принтера.
            ServerConnect.Connection.On<PrinterDTO>("AddNewPrinter", (newPrinter) => Data.UserData.AddNewPrinter(newPrinter));
            // Обработчик вызова разрешения редактирования принтера.
            ServerConnect.Connection.On("AllowPrinterEdit", () => CanEdit = true);
        }


        /// <summary>
        /// Обработчик события обновления всех данных.
        /// </summary>
        private void UserData_UpdateAllData()
        {
            // Инициализировать представление принтеров.
            PrintersView = new ListCollectionView(Data.UserData.Printers);
        }

        /// <summary>
        /// Метод фильтрации принтеров.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private bool PrinterFiltering(object obj)
        {
            if (obj is PrinterDTO item)
            {
                return item.Model.ToLower().Contains(SearchString.ToLower());
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Метод фильтрации картриджей.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private bool CartridgeFiltering(object obj)
        {
            if (obj is CartridgeDTO item)
            {
                return item.Model.ToLower().Contains(CartridgeSearch.ToLower());
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Обновляет представления совместимых и добавляемых картриджей.
        /// </summary>
        private void UpdateCartridgesViews()
        {
            CompCartridgesView = new ListCollectionView(SelectedPrinter.Compatibility.ToList());
            AddedCartridgesView = new ListCollectionView(Data.UserData.Cartridges.Where(x => !SelectedPrinter.Compatibility.Select(p => p.Id).Contains(x.Id)).ToList());
        }

        /// <summary>
        /// Проверка на несохраненные данные при изменении выбранного принтера.
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
        /// Проводит проверку принтера и отправку на сервер.
        /// </summary>
        private async void ValidateAndSendReceiptAsync()
        {
            // Провести валидацию объекта с получением списка ошибок.
            List<ValidationResult> results = SelectedPrinter.Validate(new ValidationContext(SelectedPrinter)).ToList();
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
                // Если принтер новый.
                if (SelectedPrinter.Id == 0)
                {
                    // Отправить принтер на сохранение в бд
                    await ServerConnect.Connection.InvokeAsync("AddPrinter", SelectedPrinter);
                }
                // Если принтер существующий.
                else
                {
                    // Отправить принтер на сохранение в бд
                    await ServerConnect.Connection.InvokeAsync("UpdatePrinter", SelectedPrinter);
                }
                // Закрыть режим редактирования.
                CanEdit = false;
                // Сбросить выбранный принтер.
                SelectedPrinter = null;
            }
        }

        /// <summary>
        /// Отменяет редактирование принтера.
        /// </summary>
        private async void DiscardChangesAsync()
        {
            // Отменить режим редактирования.
            CanEdit = false;
            // Если принтер новый.
            if (SelectedPrinter.Id == 0)
            {
                // Сбросить выбранный принтер.
                SelectedPrinter = null;
            }
            else
            {
                // Отправить запрос на закрытие документа.
                await ServerConnect.Connection.InvokeAsync("CloseDocument", SelectedPrinter.Id, SelectedPrinter.GetType().Name, 0);
                // Отправить запрос на получение принтера.
                await ServerConnect.Connection.InvokeAsync("GetPrinter", SelectedPrinter.Id);
            }
        }
    }
}
