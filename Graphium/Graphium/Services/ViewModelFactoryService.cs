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
        #region PROPERTIES
        private readonly Dictionary<Type, Func<ViewModelBase>> _creators = [];
        #endregion
        #region METHODS
        public ViewModelFactoryService(Create<MeasurementViewModel> measurementViewModelCreator, 
                                       Create<DataPlotterViewModel> dataPlotterViewModelCreator,
                                       Create<DataAcquisitionViewModel> dataAcquisitionViewModelCreator,
                                       Create<ChannelsConfigViewModel> channelsConfigViewModelCreator,
                                       Create<SignalManagerViewModel> signalManagerViewModelCreator)
        {
            _creators = new Dictionary<Type, Func<ViewModelBase>> {
                { typeof(MeasurementViewModel), () => measurementViewModelCreator() },
                { typeof(DataPlotterViewModel), () => dataPlotterViewModelCreator() },
                { typeof(DataAcquisitionViewModel), () => dataAcquisitionViewModelCreator() },
                { typeof(ChannelsConfigViewModel), () => channelsConfigViewModelCreator() },
                { typeof(SignalManagerViewModel), () => signalManagerViewModelCreator()}
            };
        }
        public TViewModel Create<TViewModel>() where TViewModel : ViewModelBase => (TViewModel)_creators[typeof(TViewModel)]();
        #endregion
    }
}
