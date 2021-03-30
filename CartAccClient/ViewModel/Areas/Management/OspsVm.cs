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
    /// Viewmodel вкладки управления ОСП.
    /// </summary>
    class OspsVm : MainViewModel
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
            set
            {
                CheckUnsaved();
                selectedOsp = value;
                RaisePropertyChanged(nameof(SelectedOsp));
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
                OspsView.Filter = new Predicate<object>(Filtering);
            }
        }

        /// <summary>
        /// Создать новое ОСП.
        /// </summary>
        public ICommand MakeNewOsp
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    // Создать новый объект ОСП.
                    SelectedOsp = new OspDTO()
                    {
                        Name = string.Empty,
                        Active = true
                    };
                    // Открыть режим редактирования.
                    CanEdit = true;
                },
                // Доступно если соединение установлено и уровень доступа 2 или меньше (Супервайзер).
                (canEx) => ServerConnect.Status && Data.UserData.CurrentUser.Access.Id <= 2);
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
                        SelectedOsp.Id, Data.UserData.CurrentUser.Osp.Id, SelectedOsp.GetType().Name, Data.UserData.CurrentUser.Fullname);
                },
                // Доступно если соединение установлено, выбрано ОСП и уровень доступа 2 или меньше (Супервайзер).
                (canEx) => ServerConnect.Status && SelectedOsp != null && Data.UserData.CurrentUser.Access.Id <= 2);
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
                    if (SelectedOsp.Id == 0 && Data.UserData.Osps.Any(x => x.Name == SelectedOsp.Name))
                    {
                        Alert.Show("ОСП уже существует!");
                    }
                    else
                    {
                        ValidateAndSendReceiptAsync();
                    }
                },
                // Команда доступна если есть подключение, включен режим редактирования и выбрано ОСП.
                (canEx) => ServerConnect.Status && CanEdit && SelectedOsp != null);
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
        public OspsVm()
        {
            // Инициализировать представление ОСП.
            OspsView = new ListCollectionView(Data.UserData.Osps);
            // Обработчик события обновления всех данных.
            Data.UserData.AllDataUpdated += UserData_UpdateAllData;
            // Обработчик вызова добавления ОСП.
            ServerConnect.Connection.On<OspDTO>("AddNewOsp", (newOsp) => Data.UserData.AddNewOsp(newOsp));
            // Обработчик вызова разрешения редактирования документа ОСП.
            ServerConnect.Connection.On("AllowOspEdit", () => CanEdit = true);
        }


        /// <summary>
        /// Обработчик события обновления всех данных.
        /// </summary>
        private void UserData_UpdateAllData()
        {
            // Инициализировать представление ОСП.
            OspsView = new ListCollectionView(Data.UserData.Osps);
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

        /// <summary>
        /// Проверка на несохраненные данные при изменении выбранного ОСП.
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
                        // Проверить и отправить почту на сервер.
                        ValidateAndSendReceiptAsync();
                        break;
                    case MessageBoxResult.No:
                        DiscardChangesAsync();
                        break;
                }
            }
        }

        /// <summary>
        /// Проводит проверку ОСП и отправку на сервер.
        /// </summary>
        private async void ValidateAndSendReceiptAsync()
        {
            // Провести валидацию объекта с получением списка ошибок.
            List<ValidationResult> results = SelectedOsp.Validate(new ValidationContext(SelectedOsp)).ToList();
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
                // Если ОСП новое.
                if (SelectedOsp.Id == 0)
                {
                    // Отправить ОСП на сохранение в бд.
                    await ServerConnect.Connection.InvokeAsync("AddOsp", SelectedOsp);
                }
                // Если ОСП существующее.
                else
                {
                    // Отправить ОСП на сохранение в бд.
                    await ServerConnect.Connection.InvokeAsync("UpdateOsp", SelectedOsp);
                }
                // Закрыть режим редактирования.
                CanEdit = false;
                // Сбросить выбранное ОСП.
                SelectedOsp = null;
            }
        }

        /// <summary>
        /// Отменяет редактирование почты.
        /// </summary>
        private async void DiscardChangesAsync()
        {
            // Отменить режим редактирования.
            CanEdit = false;
            // Если ОСП новое.
            if (SelectedOsp.Id == 0)
            {
                // Сбросить выбранное ОСП.
                SelectedOsp = null;
            }
            else
            {
                // Отправить запрос на закрытие документа.
                await ServerConnect.Connection.InvokeAsync("CloseDocument", SelectedOsp.Id, SelectedOsp.GetType().Name, Data.UserData.CurrentUser.Osp.Id);
                // Отправить запрос на получение ОСП.
                await ServerConnect.Connection.InvokeAsync("GetOsp", SelectedOsp.Id);
            }
        }
    }
}
