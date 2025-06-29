using System.Windows;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Graphium.ViewModels
{
    internal class BaseViewModel : INotifyPropertyChanged
    {
        public Window Window { get; set; }
        public event PropertyChangedEventHandler? PropertyChanged;

        public BaseViewModel(Window window)
        {
            Window = window;
        }

        #region METHODS
        protected void SetProperty<T>(ref T prop, T value, [CallerMemberName] string name = null!)
        {
            if (!EqualityComparer<T>.Default.Equals(prop, value))
                prop = value;
            OnPropertyChanged(name);
        }
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion
    }
}
