using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Graphium.Models
{
    public class ModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        #region METHODS
        protected void SetProperty<T>(ref T prop, T value, [CallerMemberName] string name = null!)
        {
            if (!EqualityComparer<T>.Default.Equals(prop, value)) { prop = value; }
            OnPropertyChanged(name);
        }
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion
    }
}
