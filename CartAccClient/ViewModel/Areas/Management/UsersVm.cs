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
    /// ViewModel вкладки управления пользователями.
    /// </summary>
    class UsersVm : MainViewModel
    {
        private ListCollectionView usersView, accessesView, ospsView;
        private List<UserDTO> adSearchResult;
        private UserDTO selectedUser;
        private string searchString, searchADUser;
        private bool isUserCreating, progressSearch;


        /// <summary>
        /// Представление коллекции пользователей.
        /// </summary>
        public ListCollectionView UsersView
        {
            get { return usersView; }
            set { usersView = value; RaisePropertyChanged(nameof(UsersView)); }
        }

        /// <summary>
        /// Представление коллекции уровней доступа для назначения пользователю.
        /// </summary>
        public ListCollectionView AccessesView
        {
            get { return accessesView; }
            set { accessesView = value; RaisePropertyChanged(nameof(AccessesView)); }
        }

        /// <summary>
        /// Представление коллекции ОСП для назначения пользователю.
        /// </summary>
        public ListCollectionView OspsView
        {
            get { return ospsView; }
            set { ospsView = value; RaisePropertyChanged(nameof(OspsView)); }
        }

        /// <summary>
        /// Коллекция результатов поиска пользователей в AD.
        /// </summary>
        public List<UserDTO> ADSearchResult
        {
            get { return adSearchResult; }
            private set { adSearchResult = value; RaisePropertyChanged(nameof(ADSearchResult)); }
        }

        /// <summary>
        /// выбранный пользователь в результате поиска в AD.
        /// </summary>
        public UserDTO ResultSelectedUser { get; set; }

        /// <summary>
        /// Выбранный пользователь.
        /// </summary>
        public UserDTO SelectedUser
        {
            get { return selectedUser; }
            set
            {
                // Проверка на сохранение данных поступления при редактировании.
                CheckUnsaved();
                selectedUser = value;
                // Установить ОСП выбранного пользователя.
                SelectedUserOsp = value is null ? null : Data.UserData.Osps.FirstOrDefault(x => x.Id == selectedUser.Osp?.Id);
                // Установить уровень доступа выбранного пользователя.
                SelectedUserAccess = value is null ? null : Data.UserData.Accesses.FirstOrDefault(x => x.Id == selectedUser.Access?.Id);
                RaisePropertyChanged(nameof(SelectedUser));
            }
        }

        /// <summary>
        /// ОСП выбранного пользователя.
        /// </summary>
        public OspDTO SelectedUserOsp
        {
            get
            {
                return SelectedUser?.Osp;
            }
            set
            {
                if (SelectedUser != null && SelectedUser.Osp != null && value != null)
                {
                    SelectedUser.Osp = value;
                }
                RaisePropertyChanged(nameof(SelectedUserOsp));
            }
        }

        /// <summary>
        /// Уровень доступа выбранного пользователя.
        /// </summary>
        public AccessDTO SelectedUserAccess
        {
            get
            {
                return SelectedUser?.Access;
            }
            set
            {
                if (SelectedUser != null && SelectedUser.Access != null && value != null)
                {
                    SelectedUser.Access = value;
                }
                RaisePropertyChanged(nameof(SelectedUserAccess));
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
                UsersView.Filter = new Predicate<object>(UserFiltering);
            }
        }

        /// <summary>
        /// Строка поискового запроса пользователя в AD.
        /// </summary>
        public string SearchADUser
        {
            get { return searchADUser; }
            set { searchADUser = value; RaisePropertyChanged(nameof(SearchADUser)); }
        }

        /// <summary>
        /// Состояние создания нового пользователя.
        /// </summary>
        public bool IsUserCreating
        {
            get { return isUserCreating; }
            set { isUserCreating = value; RaisePropertyChanged(nameof(IsUserCreating)); }
        }

        /// <summary>
        /// Состояние поиска нового пользователя.
        /// </summary>
        public bool ProgressSearch
        {
            get { return progressSearch; }
            set { progressSearch = value; RaisePropertyChanged(nameof(ProgressSearch)); }
        }

        /// <summary>
        /// Создать нового пользователя.
        /// </summary>
        public ICommand MakeNewUser
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    // Создать новый объект пользователя.
                    SelectedUser = new UserDTO()
                    {
                        Login = string.Empty,
                        Fullname = string.Empty,
                        Access = new AccessDTO(),
                        Osp = new OspDTO(),
                        Active = true
                    };
                    // Включить режим создания пользователя.
                    IsUserCreating = true;
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
                        SelectedUser.Id, SelectedUser.Osp.Id, SelectedUser.GetType().Name, Data.UserData.CurrentUser.Fullname);
                },
                // Доступно если соединение установлено, выбран пользователь и уровень доступа 2 или меньше (супервайзер).
                (canEx) => ServerConnect.Status && SelectedUser != null && Data.UserData.CurrentUser.Access.Id <= 2);
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
                    // Проверка на назначение администратора.
                    if (SelectedUser.Access.Id == 1 && Data.UserData.CurrentUser.Access.Id != 1)
                    {
                        Alert.Show("Назначать администраторов могут только другие администраторы.\nВыберите другой уровень доступа.");
                    }
                    else
                    {
                        // Проверить и отправить пользователя на сервер.
                        ValidateAndSendReceiptAsync();
                    }
                },
                // Команда доступна если есть подключение, включен режим редактирования и выбран пользователь.
                (canEx) => ServerConnect.Status && CanEdit && SelectedUser != null && !IsUserCreating);
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
        /// Найти пользователя.
        /// </summary>
        public ICommand SearchUser
        {
            get
            {
                return new RelayCommand(async obj =>
                {
                    ProgressSearch = true;
                    // Отправить запрос на получение пользователей из AD.
                    await ServerConnect.Connection.InvokeAsync("GetADUsers", SearchADUser);
                },
                // Команда доступна если строка поиска не пустая, есть коннект и включен режим редактирования.
                (canEx) => ServerConnect.Status && !string.IsNullOrWhiteSpace(SearchADUser) && CanEdit);
            }
        }

        /// <summary>
        /// Выбрать найденного пользователя.
        /// </summary>
        public ICommand SelectFoundUser
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    // Проверка на повтор.
                    if (Data.UserData.Users.Any(x => x.Login == ResultSelectedUser.Login))
                    {
                        Alert.Show("Пользователь уже существует!");
                    }
                    else
                    {
                        // Присвоить найденного пользователя выбранному.
                        SelectedUser = ResultSelectedUser;
                        // Выключить режим создания пользователя.
                        IsUserCreating = false;
                        // Очистить параметры поиска.
                        ADSearchResult = new List<UserDTO>();
                        SearchADUser = string.Empty;
                    }
                },
                // Команда доступна если выбран пользователь в результатах поиска и включен режим редактирования.
                (canEx) => ResultSelectedUser != null && CanEdit);
            }
        }

        /// <summary>
        /// Конструктор.
        /// </summary>
        public UsersVm()
        {
            // Инициализировать представления.
            UsersView = new ListCollectionView(Data.UserData.Users);
            AccessesView = new ListCollectionView(Data.UserData.Accesses);
            OspsView = new ListCollectionView(Data.UserData.Osps.Where(x => x.Active).ToList());

            // Обработчик события обновления всех данных.
            Data.UserData.AllDataUpdated += UserData_UpdateAllData;
            // Обработчик вызова добавления нового поступления в ОСП.
            ServerConnect.Connection.On<UserDTO>("AddNewUser", (newUser) => Data.UserData.AddNewUser(newUser));
            // Обработчик вызова разрешения редактирования документа поступления.
            ServerConnect.Connection.On("AllowUserEdit", () => CanEdit = true);
            // Обработчик вызова получения пользователей из AD.
            ServerConnect.Connection.On<List<UserDTO>>("GetADUsers", (adUsers) =>
            {
                ADSearchResult = adUsers;
                ProgressSearch = false;
            });
            // Обработчик вызова добавления ОСП.
            ServerConnect.Connection.On<OspDTO>("AddNewOsp", (newOsp) => OspsView = new ListCollectionView(Data.UserData.Osps.Where(x => x.Active).ToList()));
        }


        /// <summary>
        /// Обработчик события обновления всех данных.
        /// </summary>
        private void UserData_UpdateAllData()
        {
            // Инициализировать представления.
            UsersView = new ListCollectionView(Data.UserData.Users);
            AccessesView = new ListCollectionView(Data.UserData.Accesses);
            OspsView = new ListCollectionView(Data.UserData.Osps.Where(x => x.Active).ToList());
        }

        /// <summary>
        /// Фильтрует пользователей
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private bool UserFiltering(object obj)
        {
            if (obj is UserDTO user && !string.IsNullOrWhiteSpace(SearchString))
            {
                return user.Osp.Name.ToLower().Contains(SearchString.ToLower())
                    || user.Access.Name.ToLower().Contains(SearchString.ToLower())
                    || user.Fullname.ToLower().Contains(SearchString.ToLower());
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Проверка на несохраненные данные при изменении выбранного пользователя.
        /// </summary>
        private void CheckUnsaved()
        {
            // Если режим редактирования активен и выключен режим создания нового пользователя.
            if (CanEdit && !IsUserCreating)
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
        /// Проводит проверку пользователя и отправку на сервер.
        /// </summary>
        private async void ValidateAndSendReceiptAsync()
        {
            // Провести валидацию объекта с получением списка ошибок.
            List<ValidationResult> results = SelectedUser.Validate(new ValidationContext(SelectedUser)).ToList();
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
                // Если пользователь новый.
                if (SelectedUser.Id == 0)
                {
                    // Отправить пользователя на сохранение в бд
                    await ServerConnect.Connection.InvokeAsync("AddUser", SelectedUser);
                }
                // Если пользователь существующий.
                else
                {
                    // Отправить пользователя на сохранение в бд
                    await ServerConnect.Connection.InvokeAsync("UpdateUser", SelectedUser);
                }
                // Закрыть режим редактирования.
                CanEdit = false;
                // Сбросить выбранного пользователя.
                SelectedUser = null;
            }
        }

        /// <summary>
        /// Отменяет редактирование пользователя.
        /// </summary>
        private async void DiscardChangesAsync()
        {
            // Отменить режим редактирования.
            CanEdit = false;
            // Отменить режим создания пользователя.
            IsUserCreating = false;
            // Если пользователь новый.
            if (SelectedUser.Id == 0)
            {
                // Отменить режим редактирования.
                CanEdit = false;
                // Сбросить выбранного пользователя.
                SelectedUser = null;
            }
            else
            {
                // Отправить запрос на закрытие документа.
                await ServerConnect.Connection.InvokeAsync("CloseDocument", SelectedUser.Id, SelectedUser.GetType().Name, Data.UserData.CurrentUser.Osp.Id);
                // Отправить запрос на получение пользователя.
                await ServerConnect.Connection.InvokeAsync("GetUser", SelectedUser.Id);
            }
        }
    }
}
