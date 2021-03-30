using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Updater.Model
{
    /// <summary>
    /// Базовая реализация INPC.
    /// </summary>
    abstract class BaseVm : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
