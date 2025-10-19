using System.Windows;
using Graphium.Controls;
using Graphium.Interfaces;
using Graphium.ViewModels;
using Graphium.Views;

namespace Graphium.Services
{
    internal class ViewFactoryService : IViewFactory
    {
        #region PROPERTIES
        private readonly Dictionary<Type, Func<Window>> _creators;
        #endregion
        #region METHODS
        public ViewFactoryService(IViewModelFactory viewModelFactory)
        {
            _creators = new Dictionary<Type, Func<Window>>
            {
                { typeof(DataAcquisitionViewModel), () => new DataAcquisitionWindow() }
            };
        }
        public Window Create<TViewModel>() where TViewModel : ViewModelBase => _creators[typeof(TViewModel)]();
        #endregion
    }
}
