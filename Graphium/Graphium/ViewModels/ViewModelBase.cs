using System.Windows;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Graphium.Models;

namespace Graphium.ViewModels
{
    public class ViewModelBase : ModelBase
    {
        public Window Window { get; set; }
        public ViewModelBase(Window window)
        {
            Window = window;
        }
        #region METHODS
        #endregion
    }
}
