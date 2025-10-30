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
        private readonly CancellationTokenSource _cts = new();
        public int TabId { get; set; } = -1;
        public string? Name { get => _name; set => SetProperty(ref _name, value); }
        public bool IsMeasuring { get; private set; }
        public DataPlotterViewModel DataPlotter { get; set; }
        public ObservableCollection<SignalBase> Signals { get; set; } = [];
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
            _cts.Cancel();
            DataPlotter.StopPloting();
        }
        private void StartMeasuring()
        {
            _dataHubService.StartCapturing();

            _measurementTask = AcquireDataAsync(_cts.Token);

            IsMeasuring = true;
            DataPlotter.StartPloting();
        }
        private async Task AcquireDataAsync(CancellationToken token)
        {
            var startTime = DateTime.Now;

            try
            {
                while (!token.IsCancellationRequested)
                {
                    var dataByModule = _dataHubService.GetData();
                    var moduleCounters = new Dictionary<ModuleType, int>();

                    // calculate elapsed time
                    double elapsedTime = (DateTime.Now - startTime).TotalSeconds;

                    foreach (var signal in Signals)
                    {
                        var sourceModuleType = signal.Source;

                        if (!dataByModule.ContainsKey(sourceModuleType)) continue;
                        var sourceData = dataByModule[sourceModuleType];
                        if (sourceData == null) continue;

                        int currentCounter = moduleCounters.TryGetValue(sourceModuleType, out int c) ? c : 0;

                        switch (signal)
                        {
                            case Models.Signal si:
                                if (!sourceData.TryGetValue(currentCounter, out var list)) break;
                                si.Update(new() { { 0, list } }, elapsedTime); // <-- pass elapsed time here
                                currentCounter++;
                                break;

                            case SignalComposite sc:
                                var slice = new Dictionary<int, List<object>>();
                                for (int i = 0; i < sc.Signals.Count; i++)
                                {
                                    if (sourceData.TryGetValue(currentCounter + i, out var item))
                                        slice[i] = item;
                                }
                                sc.Update(slice, elapsedTime); // <-- pass elapsed time here
                                currentCounter += sc.Signals.Count;
                                break;
                        }

                        moduleCounters[sourceModuleType] = currentCounter;
                    }

                    await Task.Delay(10, token); // can be made configurable
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
