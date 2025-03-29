using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FHMA.ViewModels
{
    internal class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
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
    }
}
