using DataHub.Core;
using Graphium.Core;
using Graphium.Models;
using Graphium.Interfaces;
using System.Collections.ObjectModel;
using System.IO;
using System.Diagnostics;

namespace Graphium.ViewModels
{
    internal class MeasurementViewModel : ViewModelBase
    {
        #region SERVICES
        private readonly ISignalService _signalService;
        private readonly IDataHubService _dataHubService;
        private readonly IViewModelFactory _viewModelFactory;
        private readonly IDialogService _dialogService;
        private readonly IFileExportService _fileExportService;
        private readonly ILoggingService _loggingService;
        #endregion
        #region PROPERTIES
        private string? _name;
        private Task? _measurementTask;
        private int _dataPollingInterval = 16;
        private SignalAligner _signalAligner = new();
        private CancellationTokenSource? _cts = new();
        private CsvMeasurementExporter? _csvExporter;
        public int TabId { get; set; } = -1;
        public string? Name { get => _name; set => SetProperty(ref _name, value); }
        public bool IsMeasuring { get; private set; }
        public DataPlotterViewModel DataPlotter { get; set; }
        public ObservableCollection<Signal> Signals { get; set; } = [];
        #endregion
        #region RELAY_COMMANDS
        public RelayCommand StartCmd => new RelayCommand(async execute => await StartMeasuringAsync(),
                                                canExecute => !_dataHubService.IsCapturing && _signalService.Signals != null && _signalService.Signals.Any());
        public RelayCommand StopCmd => new RelayCommand(async execute => await StopMeasuringAsync(), canExecute => _dataHubService.IsCapturing);
        public RelayCommand SaveAsCSVCmd => new RelayCommand(async execute => await SaveAsCSV());
        #endregion
        #region METHODS
        public MeasurementViewModel(ISignalService signalService,
                                    IDataHubService dataHubService,
                                    IViewModelFactory viewModelFactory,
                                    IDialogService dialogService,
                                    IFileExportService fileExportService,
                                    ILoggingService loggingService)
        {
            _signalService = signalService;
            _dataHubService = dataHubService;
            _viewModelFactory = viewModelFactory;
            _dialogService = dialogService;
            _fileExportService = fileExportService;
            _loggingService = loggingService;
            DataPlotter = _viewModelFactory.Create<DataPlotterViewModel>();
        }
        public async Task StopMeasuringAsync()
        {
            _dataHubService.StopCapturing();

            _cts?.Cancel();
            IsMeasuring = false;

            if (_measurementTask != null)
            {
                try
                {
                    await _measurementTask;
                } catch (Exception ex)
                {
                    _loggingService.LogError($"Error waiting for measurement task: {ex.Message}");
                }
                _measurementTask = null;
            }
            _csvExporter?.Dispose();
            _csvExporter = null;

            _cts?.Dispose();
            _cts = null;
        }
        private async Task StartMeasuringAsync()
        {
            if (_dataHubService.IsCapturing)
            {
                _loggingService.LogWarning("Attempted to start measurement while already capturing.");
                return;
            }

            bool hasPreviousData = Signals.Any(s => s.XData.Any());

            if (hasPreviousData)
            {
                bool confirmedOverwrite = _dialogService.ShowConfirmation(
                    "Starting a new measurement will erase all unsaved data from the previous session. Do you want to continue?",
                    "Overwrite Unsaved Data?");

                if (!confirmedOverwrite) { return; }

                bool confirmedSave = _dialogService.ShowConfirmation(
                    "The previous measurement is unsaved. Would you like to save it before starting a new session?",
                    "Save Current Data?");

                if (confirmedSave) { await SaveAsCSV(suppressResumePrompt: true); } // Pass the flag

                // Ensure data is cleared outside the 'if (hasPreviousData)' block 
                // if you had a separate sync StartMeasuring, but here it's fine 
                // to clear *after* the decision process.
            }

            // Clear data and reset state
            foreach (var signal in Signals)
            {
                signal.ClearData();
            }
            DataPlotter.Reset();
            _signalAligner = new SignalAligner();

            // Start new session
            _dataHubService.StartCapturing();
            _cts = new CancellationTokenSource();
            _csvExporter = new CsvMeasurementExporter(this);
            _measurementTask = AcquireDataAsync(_cts.Token);
            IsMeasuring = true;

            DataPlotter.ResumeRefresh();
            DataPlotter.OnSignalsChanged();
        }
        private async Task SaveAsCSV(bool suppressResumePrompt = false)
        {
            bool wasRunning = IsMeasuring;
            if (wasRunning) { await StopMeasuringAsync(); }

            try
            {
                // ... file saving logic ...
                string sourceFile = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "Graphium",
                    "Measurements",
                    $"{Name}_tmpMeasurement.csv");

                await _fileExportService.ExportAsync(
                    sourceFile,
                    "measurement.csv",
                    "CSV files (*.csv)|*.csv|All files (*.*)|*.*");
            }
            finally
            {
                if (wasRunning && !suppressResumePrompt && _dialogService.ShowConfirmation("Resume measurement?", "Continue"))
                {
                    await StartMeasuringAsync();
                }
            }
        }
        private async Task AcquireDataAsync(CancellationToken token)
        {
            DateTime startTime = DateTime.Now;
            var ts = new Dictionary<Signal, HashSet<double>>();

            while (!token.IsCancellationRequested)
            {
                var dataByModule = _dataHubService.GetData();
                var moduleCounters = new Dictionary<ModuleType, int>();

                foreach (var signal in Signals)
                {
                    var sourceType = signal.Source;
                    if (!moduleCounters.ContainsKey(sourceType))
                        moduleCounters[sourceType] = 0;
                    int currentCounter = moduleCounters[sourceType];
                    if (!ts.ContainsKey(signal))
                        ts[signal] = new HashSet<double>();

                    if (dataByModule.TryGetValue(signal.Source, out var data) && data != null)
                    {
                        if (data.TryGetValue(currentCounter, out var pairs))
                        {
                            var delta = 1000.0 / signal.SamplingRate;
                            double lastTimestamp = ts[signal].Count > 0 ? ts[signal].Max() : 0;

                            foreach (var pair in pairs)
                            {
                                var xTimestamp = Math.Max(lastTimestamp + delta,
                                    (pair.timestamp - startTime).TotalMilliseconds);

                                if (ts[signal].Add(xTimestamp))
                                {
                                    signal.Update(xTimestamp, pair.value);
                                    _signalAligner.UpdateSignal(signal, xTimestamp, pair.value);
                                    lastTimestamp = xTimestamp;
                                }
                            }
                        }
                    }
                    moduleCounters[sourceType] = currentCounter + 1;
                }

                double maxTimestamp = _signalAligner.GetMaxTimestamp();

                foreach (var signal in Signals)
                {
                    double signalLastTimestamp = _signalAligner.GetLastTimestamp(signal.Source);

                    if (signalLastTimestamp < maxTimestamp)
                    {
                        var lastValue = _signalAligner.GetLastValue(signal);
                        if (lastValue != null)
                        {
                            var delta = 1000.0 / signal.SamplingRate;
                            double lastTimestamp = ts[signal].Count > 0 ? ts[signal].Max() : 0;

                            while (lastTimestamp + delta <= maxTimestamp)
                            {
                                lastTimestamp += delta;
                                if (ts[signal].Add(lastTimestamp))
                                {
                                    signal.Update(lastTimestamp, lastValue);
                                    _signalAligner.UpdateSignal(signal, lastTimestamp, lastValue);
                                }
                            }
                        }
                    }
                }

                DataPlotter.Update(maxTimestamp);

                // Export to CSV
                if (_csvExporter != null)
                {
                    await _csvExporter.ExportAsync();
                }

                await Task.Delay(_dataPollingInterval, token);
            }
        }
        #endregion
    }
}