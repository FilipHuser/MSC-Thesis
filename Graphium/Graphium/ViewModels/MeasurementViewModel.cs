using DataHub.Core;
using Graphium.Core;
using Graphium.Models;
using Graphium.Interfaces;
using System.Collections.ObjectModel;
using Microsoft.Win32;
using System.Windows;
using System.IO;
using Graphium.Services;
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
        public RelayCommand StartCmd => new RelayCommand(execute => StartMeasuring(), canExecute => !_dataHubService.IsCapturing && _signalService.Signals != null                                                                                                              && _signalService.Signals.Any());
        public RelayCommand StopCmd => new RelayCommand(execute => StopMeasuring(), canExecute => _dataHubService.IsCapturing);
        public RelayCommand SaveAsCSVCmd => new RelayCommand(execute => SaveAsCSV());
        #endregion
        #region METHODS
        public MeasurementViewModel(ISignalService signalService, 
                                    IDataHubService dataHubService, 
                                    IViewModelFactory viewModelFactory,
                                    IDialogService dialogService)
        {
            _signalService = signalService;
            _dataHubService = dataHubService;
            _viewModelFactory = viewModelFactory;
            _dialogService = dialogService;
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
        private void SaveAsCSV()
        {
            bool wasRunning = IsMeasuring;

            if (wasRunning)
            {
                StopMeasuring();
            }

            try
            {
                string sourceFile = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "Graphium",
                    "Measurements",
                    $"{Name}_tmpMeasurement.csv");

                if (!File.Exists(sourceFile))
                {
                    MessageBox.Show("No measurement file found.",
                                    "Error",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Warning);
                    return;
                }

                // Open save dialog for the user
                var dialog = new SaveFileDialog
                {
                    InitialDirectory = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)),
                    Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                    FileName = "measurement.csv"
                };

                if (dialog.ShowDialog() == true)
                {
                    File.Copy(sourceFile, dialog.FileName, overwrite: true);
                    MessageBox.Show("CSV file saved successfully.",
                                    "Success",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save CSV: {ex.Message}",
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
            finally
            {
                if (wasRunning)
                {
                    var result = MessageBox.Show("Resume measurement?",
                                                  "Continue",
                                                  MessageBoxButton.YesNo,
                                                  MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        StartMeasuring();
                    }
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

                    if (!ts.ContainsKey(signal)) { ts[signal] = new HashSet<double>(); }

                    if (dataByModule.TryGetValue(signal.Source, out var data) && data != null)
                    {

                        if (!data.TryGetValue(currentCounter, out var pairs)) { continue; }

                        int i = 0;
                        var delta = 1000.0 / signal.SamplingRate;

                        double lastTimestamp = ts[signal].Count > 0 ? ts[signal].Max() : 0;

                        foreach (var pair in pairs)
                        {
                            var xTimestamp = Math.Max(lastTimestamp + delta, (pair.timestamp - startTime).TotalMilliseconds);

                            if (!ts[signal].Add(xTimestamp))
                            {
                                Debug.WriteLine($"Duplicate timestamp: {signal.Name} {xTimestamp}");
                            }

                            signal.Update(xTimestamp, pair.value);
                            lastTimestamp = xTimestamp;
                        }

                        DataPlotter.Update(0);
                    }
                    moduleCounters[sourceType] = currentCounter + 1;

                }
                await Task.Delay(_dataPollingInterval, token);
            }
        }
        #endregion
    }
}
