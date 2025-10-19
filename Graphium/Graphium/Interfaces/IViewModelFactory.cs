using System.Windows;
using Graphium.ViewModels;

namespace Graphium.Interfaces
{
    interface IViewModelFactory
    {
        #region METHODS
        TViewModel Create<TViewModel>() where TViewModel : ViewModelBase;
        #endregion
    }
}
