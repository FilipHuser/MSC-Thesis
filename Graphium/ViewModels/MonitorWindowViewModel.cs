using System.Collections.ObjectModel;
using System.Windows.Threading;
using ScottPlot.Plottables;
using System.Configuration;
using Graphium.Models;
using System.Windows;
using ScottPlot.WPF;
using Graphium.Core;
using ScottPlot;
using DataHub;
using DataHub.Modules;
using SharpPcap;

namespace Graphium.ViewModels
{
    class MonitorWindowViewModel : BaseViewModel
    {
        #region PROPERTIES
        private DataHub.Hub _hub = new Hub();
        private DispatcherTimer _updateTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(10),
            IsEnabled = true
        };
        private DispatcherTimer _dataTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(10),
            IsEnabled = true
        };
        public WpfPlot PlotControl { get; } = new WpfPlot();
        public Dictionary<Graph , DataStreamer> GraphDataStreamers { get; set; } = new Dictionary<Graph, DataStreamer>();
        public ObservableCollection<BiologicalSignal> BiologicalSignals { get; set; } = new ObservableCollection<BiologicalSignal>();
        #endregion
        public MonitorWindowViewModel(Window window , ObservableCollection<BiologicalSignal> biologicalSignals) : base(window)
        {
            BiologicalSignals = biologicalSignals;
            Init();
        }
        
        #region METHODS
        private void Init()
        {
            var getConfigValue = (string key) => ConfigurationManager.AppSettings[key];

            if (!int.TryParse(getConfigValue("PayloadSize"), out int payloadSize) ||
                !int.TryParse(getConfigValue("CaptureDeviceIndex"), out int captureDeviceIndex))
            {
                throw new ArgumentException("Invalid configuration values for Capture Device Index or Payload Size !");
            }

            string filter = @$"udp and src host {getConfigValue("IPAddr")} and udp[4:2] > {payloadSize}";

            var packetModule = new PacketModule<RawCapture>(captureDeviceIndex, filter);
            var httpModule = new HTTPModule<string>(getConfigValue("URI"));

            _hub.AddModule(packetModule);
            _hub.AddModule(httpModule);
            _hub.StartCapturing();


            var graphs = BiologicalSignals.SelectMany(x => x.Graphs).ToList();
            PlotControl.Multiplot.RemovePlot(PlotControl.Multiplot.GetPlot(0));

            graphs.ForEach(x => {
                var plot = PlotControl.Multiplot.AddPlot();
                var streamer = plot.Add.DataStreamer(x.PointLimit);

                plot.Axes.Left.RemoveTickGenerator();
                plot.Axes.Bottom.RemoveTickGenerator();
                plot.Grid.YAxis = plot.Axes.Right;

                streamer.Axes.YAxis = plot.Axes.Right;

                plot.Axes.Left.Label.Text = x.Label ?? "~";
                GraphDataStreamers.Add(x, plot.Add.DataStreamer(x.PointLimit));
            });
            PlotControl.Multiplot.CollapseVertically();
            //PlotControl.UserInputProcessor.Disable();

            UpdateData();

            Window.Closing += (e, s) => { _hub.StopCapturing(); };
        }        
        private void UpdateData()
        {
            /*
            var bsList = BiologicalSignals.ToList();
            var bsBySource = bsList.GroupBy(x => x.Source);
            var bsStoreage = new BiologicalSignalStorage(bsList);

            _updateTimer.Tick += (s, e) =>
            {
                PlotControl.Refresh();
            };

            _dataTimer.Tick += (s, e) =>
            {
                var packets = _pcapi.GetPackets();

                foreach (var packet in packets)
                {
                    var matchingGraphs = bsBySource.FirstOrDefault(x => x.Key == packet.Source)?.SelectMany(x => x.Graphs).ToList();

                    if (matchingGraphs == null) { continue; }

                    var data = packet.ExtractData(matchingGraphs.Count);

                    if (data == null || data.Count == 0) { continue; }

                    for (int i = 0; i < matchingGraphs.Count; i++)
                    {
                        var graph = matchingGraphs.ElementAt(i);
                        int dataIndex = i % matchingGraphs.Count;

                        var values = data[dataIndex].Select(x => Convert.ToDouble(x.Item2));
                        GraphDataStreamers[graph].AddRange(values);

                        var value = DataTransformer.Transform(data[dataIndex], packet.Source);
                        bsStoreage.Add(graph, value);
                    }
                }
            };
            */
        }
        #endregion
    }
}
