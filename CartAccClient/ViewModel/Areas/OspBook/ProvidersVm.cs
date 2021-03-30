using CartAccClient.Model;
using CartAccClient.View;
using CartAccLibrary.Dto;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace CartAccClient.ViewModel
{
    /// <summary>
    /// ViewModel вкладки поставщиков.
    /// </summary>
    class ProvidersVm : MainViewModel
    {
        private ListCollectionView providersView;
        private ProviderDTO selectedProvider;
        private string searchString;

        /// <summary>
        /// Представление коллекции поставщиков.
        /// </summary>
        public ListCollectionView ProvidersView
        {
            get { return providersView; }
            set { providersView = value; RaisePropertyChanged(nameof(ProvidersView)); }
        }

        /// <summary>
        /// Выбранный поставщик.
        /// </summary>
        public ProviderDTO SelectedProvider
        {
            get { return selectedProvider; }
            set 
            {
                CheckUnsaved();
                selectedProvider = value; 
                RaisePropertyChanged(nameof(SelectedProvider)); 
            }
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
                ProvidersView.Filter = new Predicate<object>(Filtering);
            }
        }

        /// <summary>
        /// Создать нового поставщика.
        /// </summary>
        public ICommand MakeNewProvider
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    // Создать новый объект поставщика.
                    SelectedProvider = new ProviderDTO()
                    {
                        Name = string.Empty,
                        Email = string.Empty,
                        Active = true,
                        Number = 0,
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
                        SelectedProvider.Id, Data.UserData.CurrentUser.Osp.Id, SelectedProvider.GetType().Name, Data.UserData.CurrentUser.Fullname);
                },
                // Доступно если соединение установлено, выбран поставщик и уровень доступа 3 или меньше (менеджер).
                (canEx) => ServerConnect.Status && SelectedProvider != null && Data.UserData.CurrentUser.Access.Id <= 3);
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
                    if (SelectedProvider.Id == 0 && Data.UserData.Providers.Any(x => x.Name == SelectedProvider.Name))
                    {
                        Alert.Show("Поставщик уже существует!");
                    }
                    else
                    {
                        ValidateAndSendReceiptAsync();
                    }
                },
                // Команда доступна если есть подключение, включен режим редактирования и выбран поставщик.
                (canEx) => ServerConnect.Status && CanEdit && SelectedProvider != null);
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
        /// Конструктор.
        /// </summary>
        public ProvidersVm()
        {
            // Инициализировать представление поступлений.
            ProvidersView = new ListCollectionView(Data.UserData.Providers);
            // Обработчик события обновления всех данных.
            Data.UserData.AllDataUpdated += UserData_UpdateAllData;
            // Обработчик вызова добавления поставщика.
            ServerConnect.Connection.On<ProviderDTO>("AddNewProvider", (newProvider) => Data.UserData.AddNewOspProvider(newProvider));
            // Обработчик вызова разрешения редактирования документа поставщика.
            ServerConnect.Connection.On("AllowProviderEdit", () => CanEdit = true);
        }


        /// <summary>
        /// Обработчик события обновления всех данных.
        /// </summary>
        private void UserData_UpdateAllData()
        {
            // Инициализировать представление почты.
            ProvidersView = new ListCollectionView(Data.UserData.Providers);
        }

        /// <summary>
        /// Метод фильтрации.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private bool Filtering(object obj)
        {
            if (obj is ProviderDTO item)
            {
                if (int.TryParse(SearchString, out int number))
                {
                    return item.Number == number;
                }
                else
                {
                    return item.Name.ToLower().Contains(SearchString.ToLower());
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Проверка на несохраненные данные при изменении выбранного поставщика.
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
        /// Проводит проверку поставщика и отправку на сервер.
        /// </summary>
        private async void ValidateAndSendReceiptAsync()
        {
            // Провести валидацию объекта с получением списка ошибок.
            List<ValidationResult> results = SelectedProvider.Validate(new ValidationContext(SelectedProvider)).ToList();
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
                // Если поставщик новый.
                if (SelectedProvider.Id == 0)
                {
                    // Отправить поставщика на сохранение в бд
                    await ServerConnect.Connection.InvokeAsync("AddProvider", SelectedProvider);
                }
                // Если поставщик существующий.
                else
                {
                    // Отправить поставщика на сохранение в бд
                    await ServerConnect.Connection.InvokeAsync("UpdateProvider", SelectedProvider);
                }
                // Закрыть режим редактирования.
                CanEdit = false;
                // Сбросить выбранное поступление.
                SelectedProvider = null;
            }
        }

        /// <summary>
        /// Отменяет редактирование поставщика.
        /// </summary>
        private async void DiscardChangesAsync()
        {
            // Отменить режим редактирования.
            CanEdit = false;
            // Если поставщик новый.
            if (SelectedProvider.Id == 0)
            {
                // Сбросить выбранного поставщика.
                SelectedProvider = null;
            }
            else
            {
                // Отправить запрос на закрытие документа.
                await ServerConnect.Connection.InvokeAsync("CloseDocument", SelectedProvider.Id, SelectedProvider.GetType().Name, Data.UserData.CurrentUser.Osp.Id);
                // Отправить запрос на получение поставщика.
                await ServerConnect.Connection.InvokeAsync("GetProvider", SelectedProvider.Id);
            }
        }
    }
}
