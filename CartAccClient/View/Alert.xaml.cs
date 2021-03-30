using System;
using System.Windows;
using System.Windows.Controls;

namespace CartAccClient.View
{
    /// <summary>
    /// Логика взаимодействия для Alert.xaml
    /// </summary>
    public partial class Alert : Window
    {
        /// <summary>
        /// Результат диалога.
        /// </summary>
        MessageBoxResult Result = MessageBoxResult.None;

        /// <summary>
        /// Конструктор.
        /// </summary>
        public Alert()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Добавляет кнопки на форму по параметру.
        /// </summary>
        /// <param name="buttons"></param>
        private void AddButtons(MessageBoxButton buttons)
        {
            switch (buttons)
            {
                case MessageBoxButton.OK:
                    AddButton("OK", MessageBoxResult.OK);
                    break;
                case MessageBoxButton.OKCancel:
                    AddButton("OK", MessageBoxResult.OK);
                    AddButton("Отмена", MessageBoxResult.Cancel, isCancel: true);
                    break;
                case MessageBoxButton.YesNo:
                    AddButton("Да", MessageBoxResult.Yes);
                    AddButton("Нет", MessageBoxResult.No);
                    break;
                case MessageBoxButton.YesNoCancel:
                    AddButton("Да", MessageBoxResult.Yes);
                    AddButton("Нет", MessageBoxResult.No);
                    AddButton("Отмена", MessageBoxResult.Cancel, isCancel: true);
                    break;
                default:
                    throw new ArgumentException("Неизвестное значение", "buttons");
            }
        }

        /// <summary>
        /// Добавляет кнопку на форму.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="result"></param>
        /// <param name="isCancel"></param>
        private void AddButton(string text, MessageBoxResult result, bool isCancel = false)
        {
            var button = new Button() { Content = text, IsCancel = isCancel };
            button.Click += (o, args) => { Result = result; DialogResult = true; };
            ButtonContainer.Children.Add(button);
        }

        /// <summary>
        /// Вызов показа окна с сообщением, заголовком и кнопками.
        /// </summary>
        /// <param name="message">Сообщение</param>
        /// <param name="caption">Заголовок</param>
        /// <param name="buttons">Кнопки</param>
        /// <returns></returns>
        public static MessageBoxResult Show(string message, string caption, MessageBoxButton buttons)
        {
            var dialog = new Alert() { Title = caption };
            dialog.MessageContainer.Text = message;
            dialog.AddButtons(buttons);
            dialog.ShowDialog();
            return dialog.Result;
        }

        /// <summary>
        /// Вызов показа окна с сообщением.
        /// </summary>
        /// <param name="message">Сообщение</param>
        /// <returns></returns>
        public static MessageBoxResult Show(string message)
        {
            var dialog = new Alert() { Title = string.Empty };
            dialog.MessageContainer.Text = message;
            dialog.AddButtons(MessageBoxButton.OK);
            dialog.ShowDialog();
            return dialog.Result;
        }
    }
}
