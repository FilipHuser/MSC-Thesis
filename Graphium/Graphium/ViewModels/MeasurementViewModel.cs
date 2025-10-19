using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Graphium.Core;
using Graphium.Interfaces;
using Graphium.Models;
using Graphium.Services;

namespace Graphium.ViewModels
{
    internal class MeasurementViewModel : ViewModelBase
    {
        #region PROPERTIES
        private readonly ISignalService _signalService;
        private readonly IDataHubService _dataHubService;
        private readonly Create<DataPlotterViewModel> _createDataPlotterViewModel;
        private string? _name;
        public int TabId { get; set; } = -1;
        public string? Name { get => _name; set => SetProperty(ref _name, value); }
        public DataPlotterViewModel DataPlotter { get; set; }
        public ObservableCollection<SignalBase> Signals { get; set; } = [];
        #endregion
        #region RELAY_COMMANDS
        #endregion
        #region METHODS
        public MeasurementViewModel(ISignalService signalService, IDataHubService dataHubService, Create<DataPlotterViewModel> createDataPlotterViewModel)
        {
            _signalService = signalService;
            _dataHubService = dataHubService;
            _createDataPlotterViewModel = createDataPlotterViewModel;
            DataPlotter = _createDataPlotterViewModel();
        }

        #endregion
    }
}
