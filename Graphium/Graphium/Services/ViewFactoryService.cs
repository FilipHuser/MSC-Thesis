using System.Windows;
using Graphium.Controls;
using Graphium.Interfaces;
using Graphium.ViewModels;
using Graphium.Views;

namespace Graphium.Services
{
    internal class ViewFactoryService : IViewFactory
    {
        #region SERVICES
        private readonly ILoggingService _loggingService;
        #endregion
        #region PROPERTIES
        private readonly Dictionary<Type, Func<Window>> _creators;
        #endregion
        #region METHODS
        public ViewFactoryService(IViewModelFactory viewModelFactory, ILoggingService loggingService)
        {
            _loggingService = loggingService;
            _creators = new Dictionary<Type, Func<Window>>
            {
                { typeof(DataAcquisitionViewModel), () => new DataAcquisitionWindow() },
                { typeof(SignalCreatorViewModel), () => new SignalCreatorWindow() }
            };
        }
        public Window Create<TViewModel>() where TViewModel : ViewModelBase
        {
            var view = _creators[typeof(TViewModel)]();
            _loggingService.LogDebug($"Created View: {view.GetType().Name}");
            return view;
        }
        #endregion
    }
}
