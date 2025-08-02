using System.Collections.ObjectModel;
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
        private DispatcherTimer _updateTimer;
        private DispatcherTimer _dataTimer;
        private ObservableCollection<SignalBase> _signals = [];
        public string Title { get; set; }
        public UserControl Tab { get; set; }
        public WpfPlot Plot { get; set; } = new WpfPlot();
        public ObservableCollection<SignalBase> Signals { get => _signals; set { SetProperty(ref _signals, value); OnSignalsUpdate(); } }
        #region RELAY_COMMANDS
        public RelayCommand StartMeasurementCmd => new RelayCommand(execute => StartMeasurement(), canExecute => Signals.Count > 0 && !_dh.IsCapturing);
        #endregion
        #endregion
        public MeasurementTabControlVM(Window parent , string title , ref Hub dh) : base(parent)
        {
            Plot.Multiplot.RemovePlot(Plot.Multiplot.GetPlot(0));

            _dh = dh;
            Title = title;
            Tab = new MeasurementTabControl(title);
            Signals.CollectionChanged += (s,e) => {
                OnSignalsUpdate(); 
            };

            _updateTimer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromMilliseconds(10),
                IsEnabled = true,
            };

            _dataTimer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromMilliseconds(10),
                IsEnabled = true,
            };

            _updateTimer.Tick += (s, e) =>
            {
                Plot.Refresh();
            };

            _dataTimer.Tick += (s, e) =>
            {
                var packetModule = (PacketModule)_dh.Modules[typeof(PacketModule)];
                var httpModule = (HTTPModule<string>)_dh.Modules[typeof(HTTPModule<string>)];

                var packetModuleSignals = Signals.Where(x => x.Source == packetModule.GetType());
                var httpModuleSignals = Signals.Where(x => x.Source == httpModule.GetType());

                var packetData = DataProcessor.Process(packetModule, packetModuleSignals.Sum(x => x.Count));
                var httpData = DataProcessor.Process(httpModule, httpModuleSignals.Sum(x => x.Count));

                int counter = 0;

                foreach (var signal in Signals)
                {
                    Dictionary<int, List<object>>? data = null;

                    if (packetModuleSignals.Contains(signal))
                    {
                        if (packetData.Count == 0) continue;
                        data = packetData;
                    }
                    else if (httpModuleSignals.Contains(signal))
                    {
                        if (httpData.Count == 0) continue;
                        data = httpData;
                    }

                    if (data == null) continue;

                    switch (signal)
                    {
                        case Signal si:
                            si.Update(new Dictionary<int, List<object>> { { counter, data[counter] } });
                            counter++;
                            break;

                        case SignalComposite sc:
                            int innerCount = sc.Signals.Count;

                            var slice = data
                                .Where(kv => kv.Key >= counter && kv.Key < counter + innerCount)
                                .ToDictionary(kv => kv.Key, kv => kv.Value);

                            sc.Update(slice);

                            counter += innerCount;
                            break;
                    }
                }
            };
        }
        #region METHODS
        private void StartMeasurement()
        {
            _dh.StartCapturing();
        }
        private void OnSignalsUpdate()
        {
            foreach(var signal in Signals)
            {
                switch (signal)
                {
                    case Signal s:
                        Plot.Multiplot.AddPlot(s.Plot);
                        break;
                    case SignalComposite sc:
                        sc.Plots.ForEach(x => Plot.Multiplot.AddPlot(x));
                        break;
                }
            }
            Plot.Multiplot.CollapseVertically();
            Plot.UserInputProcessor.IsEnabled = false;
            Plot.Refresh();
        }
        #endregion
    }
}
