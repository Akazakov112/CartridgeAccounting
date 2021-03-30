using System.Diagnostics;
using System.Windows;
using CartAccClient.View;
using CartAccLibrary.Dto;
using CartAccLibrary.EventArgs;
using CartAccLibrary.Services;

namespace CartAccClient.Model
{
    /// <summary>
    /// Текущие данные для работы с программой.
    /// </summary>
    class AppData : BaseVm
    {
        private OspDataDTO userData;

        /// <summary>
        /// Данные пользователя.
        /// </summary>
        public OspDataDTO UserData
        {
            get { return userData; }
            set { userData = value; RaisePropertyChanged(nameof(UserData)); }
        }

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="userDataDTO"></param>
        private AppData(OspDataDTO userDataDTO)
        {
            UserData = userDataDTO;
        }


        private static AppData data;

        /// <summary>
        /// Текущие данные.
        /// </summary>
        public static AppData Data
        {
            get { return data is null ? new AppData(new OspDataDTO()) : data; }
        }

        /// <summary>
        /// Инициализирует данные.
        /// </summary>
        /// <param name="userDataDTO"></param>
        public static void Init(OspDataDTO userDataDTO)
        {
            data = new AppData(userDataDTO);
            // Подписка на событие изменения свойств текущего пользователя.
            data.UserData.CurrentUser.PropChanged += CurrentUser_PropChanged;
        }

        /// <summary>
        /// Обработчик события изменения свойства текущего пользователя.
        /// </summary>
        /// <param name="e"></param>
        public static void CurrentUser_PropChanged(UserChangeEventArgs e)
        {
            switch (e.Prop)
            {
                case UserChangeEventArgs.Props.Access:
                    Alert.Show($"У Вас изменился уровень доступа.\nНовое значение - {e.NewValue}.", "Изменение данных.", MessageBoxButton.OK);
                    break;
                case UserChangeEventArgs.Props.Active:
                    Alert.Show($"{e.NewValue}. Приложение будет закрыто без сохранения данных.\nОбратитесь к своему руководителю.", "Изменение данных.", MessageBoxButton.OK);
                    Application.Current.Shutdown();
                    break;
                case UserChangeEventArgs.Props.Fullname:
                    Alert.Show($"Изменены данные Вашей учетной записи - Ф.И.О.\nНовое значение - {e.NewValue}.", "Изменение данных.", MessageBoxButton.OK);
                    break;
                case UserChangeEventArgs.Props.Osp:
                    Alert.Show($"Изменены данные Вашей учетной записи - ОСП.\nПрограмма будет перезапущена с данными нового ОСП.", "Изменение данных.", MessageBoxButton.OK);
                    Process.Start(Application.ResourceAssembly.Location);
                    Application.Current.Shutdown();
                    break;
            }
        }
    }
}
