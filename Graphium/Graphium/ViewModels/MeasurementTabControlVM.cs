using System.Collections.ObjectModel;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using DataHub;
using DataHub.Modules;
using Graphium.Controls;
using Graphium.Core;
using Graphium.Models;
using ScottPlot;
using ScottPlot.WPF;

namespace Graphium.ViewModels
{
    internal partial class MeasurementTabControlVM : ViewModelBase
    {
        #region PROPERTIES
        private readonly Hub _dh;
        private SignalStorage? _signalStorage;
        private DispatcherTimer _updateTimer;
        private ObservableCollection<SignalBase> _signals = [];
        private Dictionary<Type, int> _signalCounts = [];
        public string Title { get; set; }
        public UserControl Tab { get; set; }
        public WpfPlot Plot { get; set; } = new WpfPlot();
        public ObservableCollection<SignalBase> Signals { get => _signals; set { SetProperty(ref _signals, value); OnSignalsUpdate(); } }
        public bool IsMeasuring => _dh.IsCapturing;
        #region RELAY_COMMANDS
        public RelayCommand StartMeasurementCmd => new RelayCommand(execute => StartMeasurement(), canExecute => Signals.Count > 0 && !_dh.IsCapturing);
        public RelayCommand StopMeasurementCmd => new RelayCommand(execute => StopMeasurement(), canExecute => _dh.IsCapturing);
        #endregion
        #endregion
        public MeasurementTabControlVM(Window parent , string title , ref Hub dh) : base(parent)
        {
            _dh = dh;
            Title = title;
            Tab = new MeasurementTabControl(title);
            Plot.Multiplot.RemovePlot(Plot.Multiplot.GetPlot(0));

            Signals.CollectionChanged += (s,e) => {
                OnSignalsUpdate(); 
            };

            _updateTimer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromMilliseconds(16),
                IsEnabled = false,
            };

            _updateTimer.Tick += (s, e) =>
            {
                Plot.Refresh();
            };

            _updateTimer.Tick += (s, e) =>
            {
                var packetModule = (PacketModule)_dh.Modules[typeof(PacketModule)];
                var httpModule = (HTTPModule<string>)_dh.Modules[typeof(HTTPModule<string>)];

                _signalCounts.TryGetValue(packetModule.GetType(), out int packetCount);
                _signalCounts.TryGetValue(httpModule.GetType(), out int httpCount);

                var packetData = DataProcessor.Process(packetModule, packetCount);
                var httpData = DataProcessor.Process(httpModule, httpCount);

                if (packetData == null && httpData == null) { return; }

                var moduleCounters = new Dictionary<Type, int>
                {
                    [typeof(PacketModule)] = 0,
                    [typeof(HTTPModule<string>)] = 0
                };

                foreach (var signal in Signals)
                {
                    var sourceType = signal.Source;
                    Dictionary<int, List<object>>? sourceData = sourceType switch
                    {
                        var t when t == typeof(PacketModule) => packetData,
                        var t when t == typeof(HTTPModule<string>) => httpData,
                        _ => null
                    };

                    if (sourceData == null) continue;
                    var currentCounter = moduleCounters[sourceType];

                    switch (signal)
                    {
                        case Signal si:
                            if (!sourceData.TryGetValue(currentCounter, out var list)) 
                            {
                                continue;
                            }

                            _signalStorage?.Add(si, list.First());
                            si.Update(new() { { 0, list } });

                            currentCounter++;
                            break;

                        case SignalComposite sc:
                            int innerCount = sc.Signals.Count;

                            var slice = new Dictionary<int, List<object>>(innerCount);
                            for (int i = 0; i < innerCount; i++)
                            {
                                if (!sourceData.TryGetValue(currentCounter + i, out var item)) {
                                    continue; 
                                }
                                slice.Add(i, item);
                             }

                            _signalStorage?.Add(sc, slice);
                            sc.Update(slice);
                            currentCounter += innerCount;
                            break;
                    }
                    moduleCounters[sourceType] = currentCounter;
                }
            };
        }
        #region METHODS
        private void StartMeasurement()
        {
            _dh.StartCapturing();
            _updateTimer.Start();
        }
        private void StopMeasurement()
        {
            _dh.StopCapturing();
        }
        private void OnSignalsUpdate()
        {
            _signalCounts = Signals.GroupBy(x => x.Source)
                                   .ToDictionary(x => x.Key, g => g.Sum(s => s.Count));

            foreach (var signal in Signals)
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
                    if (plottable is ScottPlot.Plottables.DataLogger logger)
                    {
                        logger.Color = color;
                    }
                }
            }

            _signalStorage = new SignalStorage(this);

            //Plot.Multiplot.CollapseVertically();
            Plot.UserInputProcessor.IsEnabled = false;
            Plot.Refresh();
        }
        #endregion
    }
}
