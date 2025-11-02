using Graphium.Views;
using System.Windows;
using Graphium.Controls;
using Graphium.Interfaces;
using Graphium.ViewModels;
using static Graphium.ViewModels.ViewModelBase;

namespace Graphium.Services
{
    internal class ViewModelFactoryService : IViewModelFactory
    {
        #region SERVICES
        private readonly ILoggingService _loggingService;
        #endregion
        #region PROPERTIES
        private readonly Dictionary<Type, Func<ViewModelBase>> _creators = [];
        #endregion
        #region METHODS
        public ViewModelFactoryService(Create<MeasurementViewModel> measurementViewModelCreator, 
                                       Create<DataPlotterViewModel> dataPlotterViewModelCreator,
                                       Create<DataAcquisitionViewModel> dataAcquisitionViewModelCreator,
                                       Create<ChannelsConfigViewModel> channelsConfigViewModelCreator,
                                       Create<SignalManagerViewModel> signalManagerViewModelCreator,
                                       Create<SignalCreatorViewModel> signalCreatorViewModelCreator,
                                       Create<PreferencesViewModel> preferencesViewModelCreator,
                                       ILoggingService loggingService)
        {
            _loggingService = loggingService;
            _creators = new Dictionary<Type, Func<ViewModelBase>> {
                { typeof(MeasurementViewModel), () => measurementViewModelCreator() },
                { typeof(DataPlotterViewModel), () => dataPlotterViewModelCreator() },
                { typeof(DataAcquisitionViewModel), () => dataAcquisitionViewModelCreator() },
                { typeof(ChannelsConfigViewModel), () => channelsConfigViewModelCreator() },
                { typeof(SignalManagerViewModel), () => signalManagerViewModelCreator() },
                { typeof(SignalCreatorViewModel), () => signalCreatorViewModelCreator() },
                { typeof(PreferencesViewModel), () => preferencesViewModelCreator()}
            };
        }
        public TViewModel Create<TViewModel>() where TViewModel : ViewModelBase
        {
            var vm = (TViewModel)_creators[typeof(TViewModel)]();
            _loggingService.LogDebug($"Created ViewModel: {vm.GetType().Name}");
            return vm;
        }
        #endregion
    }
}
