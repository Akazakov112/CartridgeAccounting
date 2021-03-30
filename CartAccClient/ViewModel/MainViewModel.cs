using CartAccClient.Model;
using CartAccClient.Model.Interfaces;
using CartAccLibrary.Services;

namespace CartAccClient.ViewModel
{
    /// <summary>
    /// Базовый ViewModel.
    /// </summary>
    abstract class MainViewModel : BaseVm
    {
        private bool canEdit;
        private bool isOpened;


        /// <summary>
        /// Возможность редактировать документ.
        /// </summary>
        public bool CanEdit
        {
            get { return canEdit; }
            set { canEdit = value; RaisePropertyChanged(nameof(CanEdit)); }
        }

        /// <summary>
        /// Хранит состояние области.
        /// </summary>
        public bool IsOpened
        {
            get { return isOpened; }
            set { isOpened = value; RaisePropertyChanged(nameof(IsOpened)); }
        }

        /// <summary>
        /// Конфигурация приложения.
        /// </summary>
        public IAppConfig AppConfiguration { get; }

        /// <summary>
        /// Подключение к серверу.
        /// </summary>
        public ConnectionServer ServerConnect { get; }

        /// <summary>
        /// Текущие данные приложения.
        /// </summary>
        public AppData Data { get; }

        /// <summary>
        /// Версия программы.
        /// </summary>
        public int Version { get; }

        /// <summary>
        /// Конструктор.
        /// </summary>
        public MainViewModel()
        {
            AppConfiguration = JsonFileAppConfig.Config;
            ServerConnect = ConnectionServer.Connect;
            Data = AppData.Data;
            Version = JsonFileAppConfig.GetAppBuild();
        }
    }
}
