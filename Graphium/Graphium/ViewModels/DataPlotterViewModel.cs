using ScottPlot;
using ScottPlot.WPF;
using Graphium.Interfaces;
using Graphium.Services;
using Graphium.Models;
using System.Windows.Input;
using ScottPlot.MultiplotLayouts;
using System.Windows;
using System.Windows.Threading;
using ScottPlot.Plottables;
using System.Collections.ObjectModel;
using Signal = Graphium.Models.Signal;

namespace Graphium.ViewModels
{
    internal class DataPlotterViewModel : ViewModelBase
    {
        #region SERVICES
        private readonly ILoggingService _loggingService;
        private readonly IViewModelFactory _viewModelFactory;
        #endregion
        #region PROPERTIES
        public ObservableCollection<SignalVisualizerViewModel> Visualizers { get; set; } = [];
        #endregion
        #region METHODS
        public DataPlotterViewModel(ILoggingService loggingService, IViewModelFactory viewModelFactory)
        {
            _loggingService = loggingService;
            _viewModelFactory = viewModelFactory;
            Init();
        }
        public void OnSignalsChanged(IReadOnlyCollection<Signal>? signals)
        {
            Visualizers.Clear();
            if (signals == null) return;

            foreach (var signal in signals)
            {
                if (signal == null) continue;
                Visualizers.Add(new NumericSignalViewModel(signal));
            }
        }
        private void Init()
        {
        }
        #endregion
    }
}