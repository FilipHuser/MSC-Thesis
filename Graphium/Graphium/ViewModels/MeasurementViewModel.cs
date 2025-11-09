using DataHub.Core;
using Graphium.Core;
using Graphium.Models;
using Graphium.Interfaces;
using System.Collections.ObjectModel;
using Microsoft.Win32;
using System.Windows;
using System.IO;
using Graphium.Services;

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
        private readonly TimeSpan _signalTimeout = TimeSpan.FromSeconds(5);
        private readonly HashSet<ModuleType> _warnedSources = new();
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
            var startTime = DateTime.Now;
            try
            {
                var lastValues = new Dictionary<Signal, object>();
                var lastReceivedPerSource = new Dictionary<ModuleType, DateTime>();

                while (!token.IsCancellationRequested)
                {
                    var dataByModule = _dataHubService.GetData();
                    var moduleCounters = new Dictionary<ModuleType, int>();
                    DateTime currentTime = DateTime.Now;

                    foreach (var signal in Signals)
                    {
                        var sourceModuleType = signal.Source;

                        if (dataByModule.TryGetValue(sourceModuleType, out var sourceData) && sourceData != null)
                        {
                            int currentCounter = moduleCounters.TryGetValue(sourceModuleType, out int c) ? c : 0;

                            if (sourceData.TryGetValue(currentCounter, out var list))
                            {
                                signal.Update(currentTime, list);
                                lastValues[signal] = list.First();
                                _signalStorage?.Add(signal, list.First(), currentTime);

                                currentCounter++;
                                moduleCounters[sourceModuleType] = currentCounter;

                                lastReceivedPerSource[sourceModuleType] = currentTime;
                                _warnedSources.Remove(sourceModuleType);
                            }
                            else if (lastValues.ContainsKey(signal))
                            {
                                signal.Update(currentTime, new List<object> { lastValues[signal] });
                                _signalStorage?.Add(signal, lastValues[signal], currentTime);
                            }
                        }
                    }

                    foreach (var sourceType in Signals.Select(s => s.Source).Distinct())
                    {
                        if (lastReceivedPerSource.TryGetValue(sourceType, out var lastTime))
                        {
                            if (currentTime - lastTime > _signalTimeout && !_warnedSources.Contains(sourceType))
                            {
                                _dialogService.ShowWarning(
                                    $"Data from source '{sourceType}' has not arrived for {_signalTimeout.TotalSeconds} seconds."
                                );
                                _warnedSources.Add(sourceType);
                            }
                        }
                        else
                        {
                            if (!_warnedSources.Contains(sourceType))
                            {
                                var firstCheckTime = currentTime - startTime;
                                if (firstCheckTime > _signalTimeout)
                                {
                                    _dialogService.ShowWarning(
                                        $"No data has been received yet from source '{sourceType}'."
                                    );
                                    _warnedSources.Add(sourceType);
                                }
                            }
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
