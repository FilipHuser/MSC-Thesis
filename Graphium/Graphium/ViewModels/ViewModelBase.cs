using System.Windows;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Graphium.Models;

namespace Graphium.ViewModels
{
    public class ViewModelBase : ModelBase
    {
        #region PROPERTIES
        public delegate TViewModel Create<TViewModel>() where TViewModel : ViewModelBase;
        #endregion
        #region METHODS
        #endregion
    }
}
