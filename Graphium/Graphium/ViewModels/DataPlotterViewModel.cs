using Graphium.Enums;
using Graphium.Interfaces;
using Graphium.Models;
using System.Collections.ObjectModel;

namespace Graphium.ViewModels
{
    internal class DataPlotterViewModel : ViewModelBase
    {
        #region SERVICES
        private readonly ILoggingService _loggingService;
        private readonly IViewModelFactory _viewModelFactory;
        #endregion
        #region PROPERTIES
        public ObservableCollection<ISignalVisualizerViewModel> Visualizers { get; set; } = [];
        #endregion
        #region METHODS
        public DataPlotterViewModel(ILoggingService loggingService, IViewModelFactory viewModelFactory)
        {
            _loggingService = loggingService;
            _viewModelFactory = viewModelFactory;
            Init();
        }
        public void OnSignalsChanged(IReadOnlyCollection<SignalBase>? signals)
        {
            Visualizers.Clear();
            if (signals == null) return;

            foreach (var signal in signals)
            {
                if (signal == null) continue;

                ISignalVisualizerViewModel visualizer = signal switch
                {
                    NumericSignal numeric => new NumericSignalViewModel(numeric),
                    TextSignal text => new TextSignalViewModel(text),
                    _ when signal.Type == SignalType.NaN => throw new InvalidOperationException($"Signal '{signal.Name}' has no type set."),
                    _ => throw new NotSupportedException($"SignalType {signal.Type} is not supported")
                };

                Visualizers.Add(visualizer);
            }
        }
        private void Init()
        {
        }
        #endregion
    }
}