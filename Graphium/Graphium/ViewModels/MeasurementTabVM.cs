using System.Collections.ObjectModel;
using System.IO;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using DataHub;
using DataHub.Core;
using DataHub.Modules;
using Graphium.Controls;
using Graphium.Core;
using Graphium.Models;
using Microsoft.Win32;
using ScottPlot;
using ScottPlot.WPF;

namespace Graphium.ViewModels
{
    internal partial class MeasurementTabVM : ViewModelBase
    {
        #region PROPERTIES
        private readonly Hub _dh;
        private SignalStorage? _signalStorage;
        private CancellationTokenSource? _cts;
        private ObservableCollection<SignalBase> _signals = [];
        private Dictionary<ModuleType, int> _signalCounts = new Dictionary<ModuleType, int>();
        public string Title { get; set; }
        public UserControl Tab { get; set; }
        public bool IsMeasuring { get; set; } = false;
        public ObservableCollection<PlotPanelVM> PlotPanelViewModels { get; set; } = [];
        public ObservableCollection<SignalBase> Signals { get => _signals; set { SetProperty(ref _signals, value); OnSignalsUpdate(); } }
        public event Action? MeasurementStartRequested;
        #region RELAY_COMMANDS
        public RelayCommand StartMeasurementCmd => new RelayCommand(execute => StartMeasurement(), canExecute => Signals.Count > 0 && !IsMeasuring);
        public RelayCommand StopMeasurementCmd => new RelayCommand(execute => StopMeasurement(), canExecute => IsMeasuring);
        public RelayCommand SaveAsCSVCmd => new RelayCommand(execute => SaveAsCSV());
        #endregion
        #endregion
        public MeasurementTabVM(Window parent, string title, ref Hub dh) : base(parent)
        {
            _dh = dh;
            Title = title;
            Tab = new MeasurementTabControl(title);
            Signals.CollectionChanged += (s, e) => { OnSignalsUpdate(); };
        }
        #region METHODS
        private async Task MeasurementLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var dataByModule = DataProcessor.ProcessAll(_dh.Modules.Values, _signalCounts);

                if (dataByModule != null && dataByModule.Values.Any(v => v != null))
                {
                    var dispatcher = Application.Current?.Dispatcher;
                    if (dispatcher != null)
                    {
                        dispatcher.Invoke(() =>
                        {
                            UpdateSignals(dataByModule);
                        });
                    }
                }

                await Task.Delay(16, token);
            }
        }
        private void UpdateSignals(Dictionary<ModuleType, Dictionary<int, List<object>>?> dataByModule)
        {
            var moduleCounters = new Dictionary<ModuleType, int>();

            foreach (var signal in Signals)
            {
                var sourceModuleType = signal.Source;

                if (!dataByModule.ContainsKey(sourceModuleType)) { continue; }

                var sourceData = dataByModule[sourceModuleType];
                if (sourceData == null) continue;

                int currentCounter = moduleCounters.TryGetValue(sourceModuleType, out int c) ? c : 0;

                switch (signal)
                {
                    case Signal si:
                        if (!sourceData.TryGetValue(currentCounter, out var list)) break;
                        _signalStorage?.Add(si, list.First());
                        si.Update(new() { { 0, list } });
                        currentCounter++;
                        break;

                    case SignalComposite sc:
                        var slice = new Dictionary<int, List<object>>();
                        for (int i = 0; i < sc.Signals.Count; i++)
                        {
                            if (sourceData.TryGetValue(currentCounter + i, out var item))
                                slice[i] = item;
                        }
                        _signalStorage?.Add(sc, slice);
                        sc.Update(slice);
                        currentCounter += sc.Signals.Count;
                        break;
                }

                moduleCounters[sourceModuleType] = currentCounter;
            }
            foreach (var vm in PlotPanelViewModels)
            {
                vm.PlotControl.Refresh();
            }
        }
        public void StartMeasurement()
        {
            MeasurementStartRequested?.Invoke();
            IsMeasuring = true;
            _dh.StartCapturing();
            _cts = new CancellationTokenSource();
            Task.Run(() => MeasurementLoop(_cts.Token));
        }
        public void StopMeasurement()
        {
            IsMeasuring = false;
            _dh.StopCapturing();
            _cts?.Cancel();
        }
        private void OnSignalsUpdate()
        {
            PlotPanelViewModels.Clear();

            _signalCounts = Signals.GroupBy(s => s.Source)
                                   .ToDictionary(g => g.Key, g => g.Sum(s => s.Count)) ?? new Dictionary<ModuleType, int>();

            foreach (var signal in Signals.Where(x => x.IsPlotted))
            {
                switch (signal)
                {
                    case Signal si:
                        var signalVM = new PlotPanelVM(Window, si);
                        PlotPanelViewModels.Add(signalVM);
                        break;

                    case SignalComposite sc:
                        foreach (var innerSignal in sc.Signals)
                        {
                            var innerVM = new PlotPanelVM(Window, innerSignal);
                            PlotPanelViewModels.Add(innerVM);
                        }
                        break;
                }
            }
            var palette = new ScottPlot.Palettes.Aurora();
            for (int i = 0; i < PlotPanelViewModels.Count; i++)
            {
                var plotVM = PlotPanelViewModels[i];
                var color = palette.GetColor(i);
                foreach (var plottable in plotVM.PlotControl.Plot.GetPlottables())
                {
                    if (plottable is ScottPlot.Plottables.DataStreamer streamer)
                        streamer.Color = color;
                }
                plotVM.PlotControl.Refresh();
            }

            _signalStorage = new SignalStorage(this);
        }
        private void SaveAsCSV()
        {
            StopMeasurement();
            try
            {
                string sourceFile = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "Graphium",
                    "Measurements",
                    $"{Title}_tmpMeasurement.csv");

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
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save CSV: {ex.Message}",
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
            StartMeasurement();
        }
        #endregion
    }
}
