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
    /// Viewmodel вкладки словаря картриджей.
    /// </summary>
    class CartridgesVm : MainViewModel
    {
        private ListCollectionView cartridgesView;
        private ListCollectionView addedPrintersView;
        private ListCollectionView compPrintersView;
        private CartridgeDTO selectedCartridge;
        private PrinterDTO selectedCompPrinter;
        private PrinterDTO selectedAddedPrinter;
        private string searchString;
        private string printerSearch;

        /// <summary>
        /// Представление коллекции картриджей.
        /// </summary>
        public ListCollectionView CartridgesView
        {
            get { return cartridgesView; }
            set { cartridgesView = value; RaisePropertyChanged(nameof(CartridgesView)); }
        }

        /// <summary>
        /// Представление коллекции добавляемых совместимых принтеров.
        /// </summary>
        public ListCollectionView AddedPrintersView
        {
            get { return addedPrintersView; }
            set { addedPrintersView = value; RaisePropertyChanged(nameof(AddedPrintersView)); }
        }

        /// <summary>
        /// Представление коллекции совместимых принтеров.
        /// </summary>
        public ListCollectionView CompPrintersView
        {
            get { return compPrintersView; }
            set { compPrintersView = value; RaisePropertyChanged(nameof(CompPrintersView)); }
        }

        /// <summary>
        /// Выбранный картридж.
        /// </summary>
        public CartridgeDTO SelectedCartridge
        {
            get { return selectedCartridge; }
            set
            {
                CheckUnsaved();
                selectedCartridge = value;
                RaisePropertyChanged(nameof(SelectedCartridge));
                if (selectedCartridge != null)
                {
                    UpdatePrintersViews();
                }
                SelectedCompPrinter = null;
                PrinterSearch = string.Empty;
            }
        }

        /// <summary>
        /// Выбранный совместимый принтер.
        /// </summary>
        public PrinterDTO SelectedCompPrinter
        {
            get { return selectedCompPrinter; }
            set { selectedCompPrinter = value; RaisePropertyChanged(nameof(SelectedCompPrinter)); }
        }

        /// <summary>
        /// Выбранный добавляемый совместимый принтер.
        /// </summary>
        public PrinterDTO SelectedAddedCompPrinter
        {
            get { return selectedAddedPrinter; }
            set { selectedAddedPrinter = value; RaisePropertyChanged(nameof(SelectedAddedCompPrinter)); }
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
                CartridgesView.Filter = new Predicate<object>(CartridgeFiltering);
            }
        }

        /// <summary>
        /// Строка поиска принтера.
        /// </summary>
        public string PrinterSearch
        {
            get { return printerSearch; }
            set
            {
                printerSearch = value;
                RaisePropertyChanged(nameof(PrinterSearch));
                AddedPrintersView.Filter = new Predicate<object>(PrinterFiltering);
            }
        }

        /// <summary>
        /// Создать новый картридж.
        /// </summary>
        public ICommand MakeNewCartridge
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    // Создать новый объект картриджа.
                    SelectedCartridge = new CartridgeDTO()
                    {
                        Id = 0,
                        Model = string.Empty,
                        Compatibility = new ObservableCollection<PrinterDTO>(),
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
                        SelectedCartridge.Id, 0, SelectedCartridge.GetType().Name, Data.UserData.CurrentUser.Fullname);
                },
                // Доступно если соединение установлено, выбран картридж и уровень доступа 3 или меньше (менеджер).
                (canEx) => ServerConnect.Status && SelectedCartridge != null && Data.UserData.CurrentUser.Access.Id <= 3);
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
                    if (SelectedCartridge.Id == 0 && Data.UserData.Cartridges.Any(x => x.Model == SelectedCartridge.Model))
                    {
                        Alert.Show("Картридж уже существует!");
                    }
                    else
                    {
                        ValidateAndSendReceiptAsync();
                    }
                },
                // Команда доступна если есть подключение, включен режим редактирования и выбран картридж.
                (canEx) => ServerConnect.Status && CanEdit && SelectedCartridge != null);
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
        /// Добавить совместимый принтер.
        /// </summary>
        public ICommand AddCompPrinter
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    // Добавить выбранный принтер в список совместимых.
                    SelectedCartridge.Compatibility.Add(SelectedAddedCompPrinter);
                    // Обновить представления принтеров.
                    UpdatePrintersViews();
                },
                // Команда доступна если выбран добавляемый принтер и включен режим редактирования.
                (canEx) => SelectedAddedCompPrinter != null && CanEdit);
            }
        }

        /// <summary>
        /// Удалить совместимый принтер.
        /// </summary>
        public ICommand RemoveCompPrinter
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    // Удалить выбранный совместимый принтер из списка.
                    SelectedCartridge.Compatibility.Remove(SelectedCompPrinter);
                    // Обновить представления принтеров.
                    UpdatePrintersViews();
                },
                // Команда доступна если выбран совместимый принтер в списке и включен режим редактирования.
                (canEx) => SelectedCompPrinter != null && CanEdit);
            }
        }

        /// <summary>
        /// Конструктор.
        /// </summary>
        public CartridgesVm()
        {
            // Инициализировать представление картриджей.
            CartridgesView = new ListCollectionView(Data.UserData.Cartridges);
            // Обработчик события обновления всех данных.
            Data.UserData.AllDataUpdated += UserData_UpdateAllData;
            // Обработчик вызова добавления картриджа.
            ServerConnect.Connection.On<CartridgeDTO>("AddNewCartridge", (newCartridge) => Data.UserData.AddNewCartridge(newCartridge));
            // Обработчик вызова разрешения редактирования картриджа.
            ServerConnect.Connection.On("AllowCartridgeEdit", () => CanEdit = true);
        }


        /// <summary>
        /// Обработчик события обновления всех данных.
        /// </summary>
        private void UserData_UpdateAllData()
        {
            // Инициализировать представление картриджей.
            CartridgesView = new ListCollectionView(Data.UserData.Cartridges);
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
                return item.Model.ToLower().Contains(SearchString.ToLower());
            }
            else
            {
                return false;
            }
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
                return item.Model.ToLower().Contains(PrinterSearch.ToLower());
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Обновляет представления совместимых и добавляемых принтеров.
        /// </summary>
        private void UpdatePrintersViews()
        {
            CompPrintersView = new ListCollectionView(SelectedCartridge.Compatibility.ToList());
            AddedPrintersView = new ListCollectionView(Data.UserData.Printers.Where(x => !SelectedCartridge.Compatibility.Select(p => p.Id).Contains(x.Id)).ToList());
        }

        /// <summary>
        /// Проверка на несохраненные данные при изменении выбранного картриджа.
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
        /// Проводит проверку картриджа и отправку на сервер.
        /// </summary>
        private async void ValidateAndSendReceiptAsync()
        {
            // Провести валидацию объекта с получением списка ошибок.
            List<ValidationResult> results = SelectedCartridge.Validate(new ValidationContext(SelectedCartridge)).ToList();
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
                // Если картридж новый.
                if (SelectedCartridge.Id == 0)
                {
                    // Отправить картридж на сохранение в бд
                    await ServerConnect.Connection.InvokeAsync("AddCartridge", SelectedCartridge);
                }
                // Если картридж существующий.
                else
                {
                    // Отправить картридж на сохранение в бд
                    await ServerConnect.Connection.InvokeAsync("UpdateCartridge", SelectedCartridge);
                }
                // Закрыть режим редактирования.
                CanEdit = false;
                // Сбросить выбранный картридж.
                SelectedCartridge = null;
            }
        }

        /// <summary>
        /// Отменяет редактирование картриджа.
        /// </summary>
        private async void DiscardChangesAsync()
        {
            // Отменить режим редактирования.
            CanEdit = false;
            // Если картридж новый.
            if (SelectedCartridge.Id == 0)
            {
                // Сбросить выбранный картридж.
                SelectedCartridge = null;
            }
            else
            {
                // Отправить запрос на закрытие документа.
                await ServerConnect.Connection.InvokeAsync("CloseDocument", SelectedCartridge.Id, SelectedCartridge.GetType().Name, 0);
                // Отправить запрос на получение картриджа.
                await ServerConnect.Connection.InvokeAsync("GetCartridge", SelectedCartridge.Id);
            }
        }
    }
}
