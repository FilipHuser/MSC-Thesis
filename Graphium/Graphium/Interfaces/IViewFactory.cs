using System.Windows;
using Graphium.ViewModels;

namespace Graphium.Interfaces
{
    internal interface IViewFactory
    {
        #region METHODS
        Window Create<TViewModel>() where TViewModel : ViewModelBase; 
        #endregion
    }
}
