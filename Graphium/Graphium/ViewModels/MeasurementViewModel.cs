using DataHub.Core;
using Graphium.Core;
using Graphium.Models;
using Graphium.Interfaces;
using System.Collections.ObjectModel;

namespace Graphium.ViewModels
{
    internal class MeasurementViewModel : ViewModelBase
    {
        #region SERVICES
        private readonly ISignalService _signalService;
        private readonly IDataHubService _dataHubService;
        private readonly IViewModelFactory _viewModelFactory;
        #endregion
        #region PROPERTIES
        private string? _name;
        private Task? _measurementTask;
        private int _dataPollingInterval = 16; // ms
        private SignalStorage? _signalStorage;
        private CancellationTokenSource? _cts = new();
        public int TabId { get; set; } = -1;
        public string? Name { get => _name; set => SetProperty(ref _name, value); }
        public bool IsMeasuring { get; private set; }
        public DataPlotterViewModel DataPlotter { get; set; }
        public ObservableCollection<Signal> Signals { get; set; } = [];
        #endregion
        #region RELAY_COMMANDS
        public RelayCommand StartCmd => new RelayCommand(execute => StartMeasuring(), canExecute => !_dataHubService.IsCapturing && _signalService.Signals != null
                                                                                                                                 && _signalService.Signals.Any());
        public RelayCommand StopCmd => new RelayCommand(execute => StopMeasuring(), canExecute => _dataHubService.IsCapturing);
        #endregion
        #region METHODS
        public MeasurementViewModel(ISignalService signalService, IDataHubService dataHubService, IViewModelFactory viewModelFactory)
        {
            _signalService = signalService;
            _dataHubService = dataHubService;
            _viewModelFactory = viewModelFactory;
            DataPlotter = _viewModelFactory.Create<DataPlotterViewModel>();
        }
        public void StopMeasuring()
        {
            _dataHubService.StopCapturing();
            IsMeasuring = false;
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
            _signalStorage = null;
        }
        private void StartMeasuring()
        {
            _signalStorage = new SignalStorage(Name!, Signals.ToList());
            _dataHubService.StartCapturing();
            _cts = new CancellationTokenSource();
            _measurementTask = AcquireDataAsync(_cts.Token);
            IsMeasuring = true;
        }
        private async Task AcquireDataAsync(CancellationToken token)
        {
            try
            {
                // Cache last known values for each signal
                var lastValues = new Dictionary<Signal, object>();

                while (!token.IsCancellationRequested)
                {
                    var dataByModule = _dataHubService.GetData();
                    var moduleCounters = new Dictionary<ModuleType, int>();
                    DateTime currentTime = DateTime.Now;

                    foreach (var signal in Signals)
                    {
                        var sourceModuleType = signal.Source;
                        if (!dataByModule.TryGetValue(sourceModuleType, out var sourceData) || sourceData == null)
                            continue;

                        int currentCounter = moduleCounters.TryGetValue(sourceModuleType, out int c) ? c : 0;

                        if (sourceData.TryGetValue(currentCounter, out var list))
                        {
                            // New data available
                            signal.Update(currentTime, list);
                            lastValues[signal] = list.First();
                            _signalStorage?.Add(signal, list.First());
                            currentCounter++;
                            moduleCounters[sourceModuleType] = currentCounter;
                        }
                        else if (lastValues.ContainsKey(signal))
                        {
                            // No new data, use last known value
                            signal.Update(currentTime, new List<object> { lastValues[signal] });
                            _signalStorage?.Add(signal, lastValues[signal]);
                        }
                    }

                    DataPlotter.Update();
                    await Task.Delay(_dataPollingInterval, token);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Measurement error: {ex.Message}");
            }
        }
        #endregion
    }
}
