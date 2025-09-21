using System.Collections.ObjectModel;
using System.IO;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using DataHub;
using DataHub.Modules;
using Graphium.Controls;
using Graphium.Core;
using Graphium.Models;
using Microsoft.Win32;
using ScottPlot;
using ScottPlot.WPF;

namespace Graphium.ViewModels
{
    //MEASUREMENT TAB
    internal partial class MTControlVM : ViewModelBase
    {
        #region PROPERTIES
        private readonly Hub _dh;
        private SignalStorage? _signalStorage;
        private CancellationTokenSource? _cts;
        private ObservableCollection<SignalBase> _signals = [];
        private Dictionary<Type, int> _signalCounts = [];
        public string Title { get; set; }
        public UserControl Tab { get; set; }
        public WpfPlot Plot { get; set; } = new WpfPlot();
        public ObservableCollection<SignalBase> Signals { get => _signals; set { SetProperty(ref _signals, value); OnSignalsUpdate(); } }
        public event Action? MeasurementStartRequested;
        public bool IsMeasuring { get; set; } = false;
        #region RELAY_COMMANDS
        public RelayCommand StartMeasurementCmd => new RelayCommand(execute => StartMeasurement(), canExecute => Signals.Count > 0 && !IsMeasuring);
        public RelayCommand StopMeasurementCmd => new RelayCommand(execute => StopMeasurement(), canExecute => IsMeasuring);
        public RelayCommand SaveAsCSVCmd => new RelayCommand(execute => SaveAsCSV());
        #endregion
        #endregion
        public MTControlVM(Window parent, string title, ref Hub dh) : base(parent)
        {
            _dh = dh;
            Title = title;
            Tab = new MTControl(title);
            Plot.Multiplot.RemovePlot(Plot.Multiplot.GetPlot(0));
            Signals.CollectionChanged += (s, e) => { OnSignalsUpdate(); };
        }
        #region METHODS
        private async Task MeasurementLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var (packetData, httpData) = await Task.Run(() =>
                {
                    var packetModule = (PacketModule)_dh.Modules[typeof(PacketModule)];
                    var httpModule = (HTTPModule<string>)_dh.Modules[typeof(HTTPModule<string>)];

                    _signalCounts.TryGetValue(packetModule.GetType(), out int packetCount);
                    _signalCounts.TryGetValue(httpModule.GetType(), out int httpCount);

                    return (
                        DataProcessor.Process(packetModule, packetCount),
                        DataProcessor.Process(httpModule, httpCount)
                    );
                });

                if (packetData != null || httpData != null)
                {
                    var dispatcher = Application.Current?.Dispatcher;

                    if(dispatcher != null)
                    {
                        dispatcher.Invoke(() =>
                        {
                            UpdateSignals(packetData, httpData);
                            Plot.Refresh();
                        });
                    }
                }

                await Task.Delay(16, token); 
            }
        }
        private void UpdateSignals(Dictionary<int, List<object>>? packetData, Dictionary<int, List<object>>? httpData)
        {
            if (packetData == null && httpData == null) return;

            var moduleCounters = new Dictionary<Type, int>
            {
                [typeof(PacketModule)] = 0,
                [typeof(HTTPModule<string>)] = 0
            };

            foreach (var signal in Signals)
            {
                var sourceType = signal.Source;
                var sourceData = sourceType == typeof(PacketModule) ? packetData
                               : sourceType == typeof(HTTPModule<string>) ? httpData
                               : null;

                if (sourceData == null) continue;

                int currentCounter = moduleCounters[sourceType];

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
                            if (sourceData.TryGetValue(currentCounter + i, out var item))
                                slice.Add(i, item);

                        _signalStorage?.Add(sc, slice);
                        sc.Update(slice);
                        currentCounter += sc.Signals.Count;
                        break;
                }

                moduleCounters[sourceType] = currentCounter;
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
            Plot.Multiplot.Reset();
            Plot.Multiplot.RemovePlot(Plot.Multiplot.GetPlot(0));
            _signalCounts = Signals.GroupBy(x => x.Source)
                                   .ToDictionary(x => x.Key, g => g.Sum(s => s.Count));

            foreach (var signal in Signals.Where(x => x.IsPlotted))
            {
                switch (signal)
                {
                    case Signal si:
                        Plot.Multiplot.AddPlot(si.Plot);
                        break;
                    case SignalComposite sc:
                        sc.Plots.ForEach(x => Plot.Multiplot.AddPlot(x));
                        break;
                }
            }

            var palette = new ScottPlot.Palettes.Aurora();
            var plots = Plot.Multiplot.GetPlots();
            var colors = palette.GetColors(plots.Length);

            for (int i = 0; i < plots.Length; i++)
            {
                var plot = plots[i];
                var color = colors[i];

                foreach (var plottable in plot.GetPlottables())
                {
                    if (plottable is ScottPlot.Plottables.DataStreamer streamer)
                    {
                        streamer.Color = color;
                    }
                }
            }

            _signalStorage = new SignalStorage(this);
            Plot.UserInputProcessor.IsEnabled = false;
            Plot.Refresh();
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
