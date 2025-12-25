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

            while (!token.IsCancellationRequested)
            {
                var dataByModule = _dataHubService.GetData();
                var biopacData = dataByModule[ModuleType.BIOPAC];

                if (biopacData == null || biopacData.Count == 0)
                {
                    await Task.Delay(_dataPollingInterval);
                    continue;
                }

                // Process packets in pairs to calculate deltaT
                for (int packetIndex = 0; packetIndex < biopacData.Count - 1; packetIndex++)
                {
                    var currentPacket = biopacData[packetIndex];
                    var nextPacket = biopacData[packetIndex + 1];

                    if (currentPacket.Count == 0 || nextPacket.Count == 0) continue;

                    // Get timestamps from first sample of each packet
                    var currentTimestamp = currentPacket.First().GetTimestamp();
                    var nextTimestamp = nextPacket.First().GetTimestamp();

                    // Calculate time delta between packets
                    TimeSpan deltaT = nextTimestamp - currentTimestamp;

                    // Calculate time increment per sample within the packet
                    TimeSpan sampleIncrement = currentPacket.Count > 1
                        ? deltaT / currentPacket.Count
                        : TimeSpan.Zero;

                    // Process all samples in current packet
                    for (int sampleIndex = 0; sampleIndex < currentPacket.Count; sampleIndex++)
                    {
                        var sample = currentPacket[sampleIndex];

                        // Calculate interpolated timestamp for this sample
                        var sampleTimestamp = currentTimestamp + (sampleIncrement * sampleIndex);

                        // Update all channels for this sample
                        foreach (var channel in sample.Channels)
                        {
                            var signal = channel.Key;
                            var value = channel.Value;
                            var xVal = (sampleTimestamp - start).TotalMilliseconds;

                            signal.Update(xVal, value);
                        }
                    }
                }

                // Handle the last packet (if there's only one packet or process the final one)
                if (biopacData.Count > 0)
                {
                    var lastPacket = biopacData.Last();
                    // You might want to process this differently or wait for the next iteration
                    // to get a proper deltaT for it
                }

                await Task.Delay(_dataPollingInterval);
            }
        }
        #endregion
    }
}