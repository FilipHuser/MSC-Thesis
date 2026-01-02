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
        private readonly IMeasurementExportService _measurementExportService;
        private readonly ILoggingService _loggingService;
        #endregion
        #region PROPERTIES
        private string? _name;
        private Task? _measurementTask;
        private int _dataPollingInterval = 10;
        private SignalAligner _signalAligner = new();
        private CancellationTokenSource? _cts = new();
        private readonly Stopwatch _globalClock = new Stopwatch();

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
                                    IMeasurementExportService measurementExportService,
                                    ILoggingService loggingService)
        {
            _signalService = signalService;
            _dataHubService = dataHubService;
            _viewModelFactory = viewModelFactory;
            _dialogService = dialogService;
            _fileExportService = fileExportService;
            _measurementExportService = measurementExportService;
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
                }
                catch (Exception ex)
                {
                    _loggingService.LogError($"Error waiting for measurement task: {ex.Message}");
                }
                _measurementTask = null;
            }

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

                if (confirmedSave) { await SaveAsCSV(suppressResumePrompt: true); }
            }

            foreach (var signal in Signals) { signal.ClearData(); }

            DataPlotter.Reset();
            _signalAligner = new SignalAligner();

            _globalClock.Restart();
            _dataHubService.StartCapturing();
            _cts = new CancellationTokenSource();
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
            var start = DateTime.Now;
            var signalsBySource = Signals.GroupBy(x => x.Source).ToDictionary(x => x.Key, x => x.ToList());
            var masterSource = signalsBySource
                .Select(x => new { Source = x.Key, MaxSamplingRate = x.Value.Max(s => s.SamplingRate) })
                .OrderByDescending(x => x.MaxSamplingRate)
                .First()
                .Source;

            var allSignalsSet = new HashSet<Signal>(Signals);

            // Just one line - service handles everything!
            using var csvWriter = _measurementExportService.CreateCsvWriter(this);

            while (!token.IsCancellationRequested)
            {
                var dataByModule = _dataHubService.GetData();
                var masterSourceData = dataByModule.TryGetValue(masterSource, out var data) ? data : null;

                if (masterSourceData == null || masterSourceData.Count == 0)
                {
                    await Task.Delay(_dataPollingInterval);
                    continue;
                }

                // Update SignalAligner with latest values from all sources
                foreach (var sourceData in dataByModule)
                {
                    var groups = sourceData.Value;
                    if (groups == null || groups.Count == 0) { continue; }

                    var lastGroup = groups[groups.Count - 1];
                    if (lastGroup.Count > 0)
                    {
                        var latestSample = lastGroup[lastGroup.Count - 1];
                        foreach (var channel in latestSample.Channels)
                        {
                            _signalAligner.UpdateSignal(channel.Key, channel.Value);
                        }
                    }
                }

                for (int groupIndex = 0; groupIndex < masterSourceData.Count - 1; groupIndex++)
                {
                    var currentGroup = masterSourceData[groupIndex];
                    var nextGroup = masterSourceData[groupIndex + 1];

                    if (currentGroup.Count == 0 || nextGroup.Count == 0) { continue; }

                    var currentTimestamp = currentGroup[0].GetTimestamp();
                    var nextTimestamp = nextGroup[0].GetTimestamp();
                    TimeSpan deltaT = nextTimestamp - currentTimestamp;
                    TimeSpan sampleIncrement = currentGroup.Count > 1 ? deltaT / currentGroup.Count : TimeSpan.Zero;

                    for (int sampleIndex = 0; sampleIndex < currentGroup.Count; sampleIndex++)
                    {
                        var sample = currentGroup[sampleIndex];
                        var sampleTimestamp = currentTimestamp + (sampleIncrement * sampleIndex);
                        var xVal = (sampleTimestamp - start).TotalMilliseconds;

                        var rowValues = new Dictionary<Signal, object>();

                        // Update signals present in this sample
                        foreach (var channel in sample.Channels)
                        {
                            channel.Key.Update(xVal, channel.Value);
                            rowValues[channel.Key] = channel.Value;
                        }

                        // Update all other signals with their last known values
                        foreach (var sig in allSignalsSet)
                        {
                            if (!sample.Channels.ContainsKey(sig))
                            {
                                var val = _signalAligner.GetLastValue(sig);
                                if (val != null)
                                {
                                    sig.Update(xVal, val);
                                    rowValues[sig] = val;
                                }
                            }
                        }

                        // Write row using the service
                        await csvWriter.WriteRowAsync(sampleTimestamp, rowValues);
                    }
                }

                // Flush periodically to ensure data is written
                await csvWriter.FlushAsync();

                await Task.Delay(_dataPollingInterval);
            }
        }
        #endregion
    }
}