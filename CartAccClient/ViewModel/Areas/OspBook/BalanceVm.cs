using CartAccClient.Model;
using CartAccLibrary.Dto;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;

namespace CartAccClient.ViewModel
{
    /// <summary>
    /// ViewModel вкладки баланса.
    /// </summary>
    class BalanceVm : MainViewModel
    {
        private ListCollectionView balanceView, inventBalanceView;
        private BalanceDTO selectedBalance;
        private bool isInventMode;
        private string searchString;

        /// <summary>
        /// Представление коллекции баланса.
        /// </summary>
        public ListCollectionView BalanceView
        {
            get { return balanceView; }
            set { balanceView = value; RaisePropertyChanged(nameof(BalanceView)); }

        }

        /// <summary>
        /// Представление коллекции баланса для инвентаризации.
        /// </summary>
        public ListCollectionView InventBalanceView
        {
            get { return inventBalanceView; }
            set { inventBalanceView = value; RaisePropertyChanged(nameof(InventBalanceView)); }
        }

        /// <summary>
        /// Список картриджей для инвентаризации.
        /// </summary>
        private List<InventBalanceDTO> InventBalances { get; set; }

        /// <summary>
        /// Выбранный баланс.
        /// </summary>
        public BalanceDTO SelectedBalance
        {
            get { return selectedBalance; }
            set { selectedBalance = value; RaisePropertyChanged(nameof(SelectedBalance)); }
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
                BalanceView.Filter = new Predicate<object>(Filtering);
                InventBalanceView.Filter = new Predicate<object>(Filtering);
            }
        }

        /// <summary>
        /// Включить использование картриджа.
        /// </summary>
        public ICommand EnableUse
        {
            get
            {
                return new RelayCommand(async obj =>
                {
                    SelectedBalance.InUse = true;
                    // Отправить запрос на сервер на добавление картриджа баланса в используемые.
                    await ServerConnect.Connection.InvokeAsync("UpdateBalance", SelectedBalance);
                },
                // Доступно если есть подключение, выбран баланс, он не используется, уровень доступа Менеджер или выше и выключен режим инвентаризации.
                (canEx) => ServerConnect.Status && !IsInventMode && SelectedBalance != null && !SelectedBalance.InUse && Data.UserData.CurrentUser.Access.Id <= 3);
            }
        }

        /// <summary>
        /// Выключить использование картриджа.
        /// </summary>
        public ICommand DisableUse
        {
            get
            {
                return new RelayCommand(async obj =>
                {
                    SelectedBalance.InUse = false;
                    // Отправить запрос на сервер на исключение картриджа баланса из используемых.
                    await ServerConnect.Connection.InvokeAsync("UpdateBalance", SelectedBalance);
                },
                // Доступно если есть подключение, выбран баланс и он используется, уровень доступа Менеджер или выше и выключен режим инвентаризации.
                (canEx) => ServerConnect.Status && !IsInventMode && SelectedBalance != null && SelectedBalance.InUse && Data.UserData.CurrentUser.Access.Id <= 3);
            }
        }

        /// <summary>
        /// Запустить режим инвентаризации.
        /// </summary>
        public ICommand StartInventMode
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    // Создать список баланса для инвентаризации из текущего баланса.
                    InventBalances = Data.UserData.Balance.Where(x => x.InUse).Select(p => new InventBalanceDTO()
                    {
                        Id = p.Id,
                        Cartridge = p.Cartridge,
                        Count = p.Count,
                        InUse = p.InUse,
                        FactCount = null
                    }).ToList();
                    InventBalanceView = new ListCollectionView(InventBalances);
                    IsInventMode = true;
                },
                // Доступно если режим инвентаризации неактивен, есть картриджи на балансе и уровень доступа Менеджер или выше..
                (canEx) => IsInventMode == false && Data.UserData.Balance.Any() && Data.UserData.CurrentUser.Access.Id <= 3);
            }
        }

        /// <summary>
        /// Сохранить данные инвентаризации.
        /// </summary>
        public ICommand SaveInventResults
        {
            get
            {
                return new RelayCommand(async obj =>
                {
                    // Отправить запрос на сервер на запись результатов инвентаризации.
                    await ServerConnect.Connection.InvokeAsync("SaveInventBalance", Data.UserData.CurrentUser.Osp.Id, Data.UserData.CurrentUser.Id, InventBalances);
                    // Выйти из режима инвентаризации.
                    IsInventMode = false;
                },
                // Доступно если соединение установлено, режим инвентаризации активен и если нет картриджей с пустым фактическим значением.
                (canEx) => ServerConnect.Status && IsInventMode == true && !InventBalances.Where(x => x.FactCount is null).Any());
            }
        }

        /// <summary>
        /// Закрыть режим инвентаризации.
        /// </summary>
        public ICommand StopInventMode
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    IsInventMode = false;
                },
                // Доступно если режим инвентаризации активен.
                (canEx) => IsInventMode == true);
            }
        }

        /// <summary>
        /// Статус режима инвентаризации.
        /// </summary>
        public bool IsInventMode
        {
            get { return isInventMode; }
            set { isInventMode = value; RaisePropertyChanged(nameof(IsInventMode)); }
        }

        /// <summary>
        /// Конструктор.
        /// </summary>
        public BalanceVm()
        {
            InventBalances = new List<InventBalanceDTO>();
            BalanceView = new ListCollectionView(Data.UserData.Balance);
            InventBalanceView = new ListCollectionView(InventBalances);
            SearchString = string.Empty;
            // Обработчик события обновления всех данных.
            Data.UserData.AllDataUpdated += UserData_UpdateAllData;
        }


        /// <summary>
        /// Метод фильтрации.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private bool Filtering(object obj)
        {
            if (obj is BalanceDTO item)
            {
                return item.Cartridge.Model.ToLower().Contains(SearchString.ToLower());
            }
            else if (obj is InventBalanceDTO inv)
            {
                return inv.Cartridge.Model.ToLower().Contains(SearchString.ToLower());
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Обработчик события обновления всех данных.
        /// </summary>
        private void UserData_UpdateAllData()
        {
            // Инициализировать представление баланса.
            BalanceView = new ListCollectionView(Data.UserData.Balance);
        }
    }
}
