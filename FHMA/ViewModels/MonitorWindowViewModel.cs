using System.Collections.ObjectModel;
using System.Configuration;
using System.Windows;
using System.Windows.Threading;
using FHMA.Core;
using FHMA.Models;
using ScottPlot;
using ScottPlot.Plottables;
using ScottPlot.WPF;

namespace FHMA.ViewModels
{
    class MonitorWindowViewModel : BaseViewModel
    {
        #region PROPERTIES
        private FHAPILib.FHAPI _fhapi = new FHAPILib.FHAPI();
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
        public ObservableCollection<BiometricSignal> BiometricSignals { get; set; } = new ObservableCollection<BiometricSignal>();
        #endregion
        public MonitorWindowViewModel(Window window , ObservableCollection<BiometricSignal> biometricSignals) : base(window)
        {
            BiometricSignals = biometricSignals;
            Init();
        }
        
        #region METHODS
        private void Init()
        {
            if (!int.TryParse(ConfigurationManager.AppSettings["CaptureDeviceIndex"], out int cdi) ||
                !int.TryParse(ConfigurationManager.AppSettings["MaxChannels"], out int maxChannels))
            {
                throw new ArgumentException("Invalid configuration values for CaptureDeviceIndex or MaxChannels.");
            }

            string filter = @$"udp and src host {ConfigurationManager.AppSettings["BiopacIpAddr"]} or
                               src host {ConfigurationManager.AppSettings["EmotiveIpAddr"]} or
                               src host {ConfigurationManager.AppSettings["StationIpAddr"]} and udp[4:2] > 18";

            _fhapi.SetDeviceIndex(cdi);
            _fhapi.SetFilter(filter);
            _fhapi.StartCapturing();
            
            var graphs = BiometricSignals.SelectMany(x => x.Graphs).ToList();
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

            Window.Closing += (e, s) => { _fhapi.StopCapturing(); };
        }        
        private void UpdateData()
        {
            var bsList = BiometricSignals.ToList();
            var bsBySource = bsList.GroupBy(x => x.Source);
            var bsStoreage = new BiometricSignalStorage(bsList);

            _updateTimer.Tick += (s, e) =>
            {
                PlotControl.Refresh();
            };

            _dataTimer.Tick += (s, e) =>
            {
                var packets = _fhapi.GetPackets();

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
        }
        #endregion
    }
}
