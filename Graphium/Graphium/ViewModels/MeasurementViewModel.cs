using DataHub.Core;
using Graphium.Core;
using Graphium.Models;
using Graphium.Interfaces;
using System.Collections.ObjectModel;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks.Dataflow;
using System.Runtime.InteropServices;
using System.Collections.Specialized;

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
        private DateTime _measurementStart;
        private int _dataPollingInterval = 10;
        private string _elapsedTime = "00:00:00";
        private SignalAligner _signalAligner = new();
        private CancellationTokenSource? _cts = new();
        private System.Timers.Timer? _clockTimer;
        public int TabId { get; set; } = -1;
        public bool IsMeasuring { get; private set; }
        public DataPlotterViewModel DataPlotter { get; set; }
        public ObservableCollection<SignalBase> Signals { get; set; } = [];
        public string? Name { get => _name; set => SetProperty(ref _name, value); }
        public ObservableCollection<SourceStatus> SourceStatuses { get; set; } = [];
        public string ElapsedTime { get => _elapsedTime; set => SetProperty(ref _elapsedTime, value); }
        #endregion
        #region RELAY_COMMANDS
        public RelayCommand StartCmd => new RelayCommand(async execute => await StartMeasuringAsync(),
                                                canExecute => !_dataHubService.IsCapturing && _signalService.Signals != null && _signalService.Signals.Any());
        public RelayCommand SaveAsCSVCmd => new RelayCommand(async execute => await SaveAsCSV());
        public RelayCommand StopCmd => new RelayCommand(async execute => await StopMeasuringAsync(), canExecute => _dataHubService.IsCapturing);
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
            Signals.CollectionChanged += OnSignalsCollectionChanged;
        }
        public async Task SaveAsCSV(bool suppressResumePrompt = false)
        {
            bool wasRunning = IsMeasuring;
            if (wasRunning) await StopMeasuringAsync();
            try
            {
                await _measurementExportService.SaveAsync(this);
            }
            finally
            {
                if (wasRunning && !suppressResumePrompt && _dialogService.ShowConfirmation("Resume measurement?", "Continue"))
                    await StartMeasuringAsync();
            }
        }
        public async Task StopMeasuringAsync()
        {
            _clockTimer?.Stop();
            _clockTimer?.Dispose();
            _clockTimer = null;
            ElapsedTime = "00:00:00";
            DataPlotter.StopRendering();
            _dataHubService.StopCapturing();
            _cts?.Cancel();
            IsMeasuring = false;
            foreach (var status in SourceStatuses) { status.IsActive = false; }
            if (_measurementTask != null)
            {
                try { await _measurementTask; }
                catch (Exception ex) { _loggingService.LogError($"Error waiting for measurement task: {ex.Message}"); }
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
            SourceStatuses.Clear();
            var uniqueSources = Signals.Select(s => s.Source).Distinct();

            foreach (var source in uniqueSources)
            {
                var samplingRate = _dataHubService.GetSamplingRate(source);
                var periodMs = 1000.0 / samplingRate;
                var timeoutMs = (int)Math.Clamp(periodMs * 3, 500, 3000);
                SourceStatuses.Add(new SourceStatus(timeoutMs) { Type = source, IsActive = false });
            }

            _signalAligner = new SignalAligner();
            _measurementStart = DateTime.Now;
            _clockTimer = new System.Timers.Timer(1000);
            _clockTimer.Elapsed += (_, _) =>
                System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
                    ElapsedTime = (DateTime.Now - _measurementStart).ToString(@"hh\:mm\:ss"));
            _clockTimer.Start();
            _clockTimer.Start();
            _dataHubService.StartCapturing();
            _cts = new CancellationTokenSource();
            _measurementTask = AcquireDataAsync(_cts.Token);
            DataPlotter.StartRendering();
            IsMeasuring = true;
        }
        private async Task AcquireDataAsync(CancellationToken token)
        {
            var start = DateTime.Now;
            var signalsBySource = Signals.GroupBy(x => x.Source).ToDictionary(x => x.Key, x => x.ToList());
            var masterSource = signalsBySource
                .Select(x => new { Source = x.Key, SamplingRate = _dataHubService.GetSamplingRate(x.Key) })
                .OrderByDescending(x => x.SamplingRate)
                .First()
                .Source;

            var slaveSignals = new HashSet<SignalBase>(Signals.Where(x => x.Source != masterSource));

            using var csvWriter = _measurementExportService.CreateCsvWriter(this);

            List<Sample>? bufferedGroup = null;

            while (!token.IsCancellationRequested)
            {
                var dataByModule = _dataHubService.GetData();

                foreach (var status in SourceStatuses)
                {
                    if (dataByModule.TryGetValue(status.Type, out var moduleData) && moduleData.Count > 0)
                        status.MarkDataReceived();
                    status.UpdateStatus();
                }

                var masterSourceData = dataByModule.TryGetValue(masterSource, out var data) ? data : null;
                var slaveSourceData = dataByModule.Where(kvp => kvp.Key != masterSource);

                if (masterSourceData == null || masterSourceData.Count == 0)
                {
                    await Task.Delay(_dataPollingInterval);
                    continue;
                }

                foreach (var sourceData in slaveSourceData)
                {
                    var groups = sourceData.Value;
                    if (groups == null || groups.Count == 0) continue;
                    var lastGroup = groups[groups.Count - 1];
                    if (lastGroup.Count > 0)
                    {
                        var latestSample = lastGroup[lastGroup.Count - 1];
                        foreach (var channel in latestSample.Channels)
                            _signalAligner.UpdateSignal(channel.Key, channel.Value);
                    }
                }

                var groupsToProcess = new List<List<Sample>>();

                if (bufferedGroup != null)
                    groupsToProcess.Add(bufferedGroup);

                groupsToProcess.AddRange(masterSourceData);

                for (int groupIndex = 0; groupIndex < groupsToProcess.Count - 1; groupIndex++)
                {
                    var currentGroup = groupsToProcess[groupIndex];
                    var nextGroup = groupsToProcess[groupIndex + 1];

                    if (currentGroup.Count == 0 || nextGroup.Count == 0) continue;

                    var currentTimestamp = currentGroup[0].GetTimestamp();
                    var nextTimestamp = nextGroup[0].GetTimestamp();
                    TimeSpan deltaT = nextTimestamp - currentTimestamp;
                    TimeSpan sampleIncrement = currentGroup.Count > 1 ? deltaT / currentGroup.Count : TimeSpan.Zero;

                    for (int sampleIndex = 0; sampleIndex < currentGroup.Count; sampleIndex++)
                    {
                        var sample = currentGroup[sampleIndex];
                        var sampleTimestamp = currentTimestamp + (sampleIncrement * sampleIndex);
                        var xVal = (sampleTimestamp - start).TotalMilliseconds;

                        var rowValues = new Dictionary<SignalBase, object>();

                        foreach (var channel in sample.Channels)
                        {
                            var signal = channel.Key;
                            if (signal.IsPlotted)
                                signal.Update(xVal, channel.Value);
                            rowValues[channel.Key] = channel.Value;
                        }

                        foreach (var sig in slaveSignals)
                        {
                            if (!sample.Channels.ContainsKey(sig))
                            {
                                var val = _signalAligner.GetLastValue(sig);
                                if (val != null)
                                {
                                    if (sig.IsPlotted)
                                        sig.Update(xVal, val);
                                    rowValues[sig] = val;
                                }
                            }
                        }

                        await csvWriter.WriteRowAsync(sampleTimestamp, rowValues);
                    }
                }

                bufferedGroup = groupsToProcess.Count > 0 ? groupsToProcess[^1] : null;

                await csvWriter.FlushAsync();
                await Task.Delay(_dataPollingInterval);
            }
        }
        private void OnSignalsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => DataPlotter.OnSignalsChanged(Signals);
        #endregion
    }
}