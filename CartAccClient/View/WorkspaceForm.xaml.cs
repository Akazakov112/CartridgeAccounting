using System;
using System.Windows;

namespace CartAccClient.View
{
    /// <summary>
    /// Логика взаимодействия для WorkspaceForm.xaml
    /// </summary>
    public partial class WorkspaceForm : Window
    {
        private WindowState prevState;

        public WorkspaceForm()
        {
            InitializeComponent();
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                this.ShowInTaskbar = false;
                Hide();
            }
            else 
            {
                prevState = WindowState;
            }     
        }

        private void Window_TrayMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            this.ShowInTaskbar = true;
            Show();
            WindowState = prevState;
        }
    }
}
