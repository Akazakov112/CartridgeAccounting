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
    /// ViewModel вкладки электронной почты.
    /// </summary>
    class EmailsVm : MainViewModel
    {
        private ListCollectionView emailsView;
        private EmailDTO selectedEmail;
        private string searchString;

        /// <summary>
        /// Представление коллекции почты.
        /// </summary>
        public ListCollectionView EmailsView
        {
            get { return emailsView; }
            set { emailsView = value; RaisePropertyChanged(nameof(EmailsView)); }
        }

        /// <summary>
        /// Выбранная почта.
        /// </summary>
        public EmailDTO SelectedEmail
        {
            get { return selectedEmail; }
            set
            {
                CheckUnsaved();
                selectedEmail = value;
                RaisePropertyChanged(nameof(SelectedEmail));
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
                EmailsView.Filter = new Predicate<object>(Filtering);
            }
        }

        /// <summary>
        /// Создать новую почту.
        /// </summary>
        public ICommand MakeNewEmail
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    // Создать новый объект почты.
                    SelectedEmail = new EmailDTO()
                    {
                        Address = string.Empty,
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
                        SelectedEmail.Id, Data.UserData.CurrentUser.Osp.Id, SelectedEmail.GetType().Name, Data.UserData.CurrentUser.Fullname);
                },
                // Доступно если соединение установлено, выбрана почта и уровень доступа 3 или меньше (менеджер).
                (canEx) => ServerConnect.Status && SelectedEmail != null && Data.UserData.CurrentUser.Access.Id <= 3);
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
                    if (SelectedEmail.Id == 0 && Data.UserData.Emails.Any(x => x.Address == SelectedEmail.Address))
                    {
                        Alert.Show("Почта уже добавлена!");
                    }
                    else
                    {
                        ValidateAndSendReceiptAsync();
                    }
                },
                // Команда доступна если есть подключение, включен режим редактирования и выбрана почта.
                (canEx) => ServerConnect.Status && CanEdit && SelectedEmail != null);
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
        public EmailsVm()
        {
            // Инициализировать представление почты.
            EmailsView = new ListCollectionView(Data.UserData.Emails);
            // Обработчик события обновления всех данных.
            Data.UserData.AllDataUpdated += UserData_UpdateAllData;
            // Обработчик вызова добавления почты.
            ServerConnect.Connection.On<EmailDTO>("AddNewEmail", (newEmail) => Data.UserData.AddNewOspEmail(newEmail));
            // Обработчик вызова разрешения редактирования документа почты.
            ServerConnect.Connection.On("AllowEmailEdit", () => CanEdit = true);
        }


        /// <summary>
        /// Обработчик события обновления всех данных.
        /// </summary>
        private void UserData_UpdateAllData()
        {
            // Инициализировать представление почты.
            EmailsView = new ListCollectionView(Data.UserData.Emails);
        }

        /// <summary>
        /// Метод фильтрации.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private bool Filtering(object obj)
        {
            // Если объекты не null.
            if (obj is EmailDTO item && !string.IsNullOrEmpty(SearchString))
            {
                return item.Address.ToLower().Contains(SearchString.ToLower());
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Проверка на несохраненные данные при изменении выбранной почты.
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
        /// Проводит проверку почты и отправку на сервер.
        /// </summary>
        private async void ValidateAndSendReceiptAsync()
        {
            // Провести валидацию объекта с получением списка ошибок.
            List<ValidationResult> results = SelectedEmail.Validate(new ValidationContext(SelectedEmail)).ToList();
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
                // Если почта новая.
                if (SelectedEmail.Id == 0)
                {
                    // Отправить почту на сохранение в бд
                    await ServerConnect.Connection.InvokeAsync("AddEmail", SelectedEmail);
                }
                // Если почта существующая.
                else
                {
                    // Отправить почту на сохранение в бд
                    await ServerConnect.Connection.InvokeAsync("UpdateEmail", SelectedEmail);
                }
                // Закрыть режим редактирования.
                CanEdit = false;
                // Сбросить выбранную почту.
                SelectedEmail = null;
            }
        }

        /// <summary>
        /// Отменяет редактирование почты.
        /// </summary>
        private async void DiscardChangesAsync()
        {
            // Отменить режим редактирования.
            CanEdit = false;
            // Если почта новая.
            if (SelectedEmail.Id == 0)
            {
                // Сбросить выбранную почту.
                SelectedEmail = null;
            }
            else
            {
                // Отправить запрос на закрытие документа.
                await ServerConnect.Connection.InvokeAsync("CloseDocument", SelectedEmail.Id, SelectedEmail.GetType().Name, Data.UserData.CurrentUser.Osp.Id);
                // Отправить запрос на получение почты.
                await ServerConnect.Connection.InvokeAsync("GetEmail", SelectedEmail.Id);
            }
        }
    }
}
