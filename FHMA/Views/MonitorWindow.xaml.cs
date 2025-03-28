using System.Collections.ObjectModel;
using System.Configuration;
using System.Windows;
using System.Windows.Threading;
using FHAPILib;
using FHMA.Models;
using FHMA.ViewModels;
using ScottPlot.WPF;

namespace FHMA.Views
{
    /// <summary>
    /// Interaction logic for MonitorWindow.xaml
    /// </summary>
    public partial class MonitorWindow : Window
    {
        private readonly FHAPILib.FHAPI _fhapi;
        private readonly MonitorWindowViewModel _vm;
        private DispatcherTimer _updateTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(10), IsEnabled = true
        };
        private DispatcherTimer _dataTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(10), IsEnabled = true
        };


        public MonitorWindow(ObservableCollection<Graph> graphs)
        {
            InitializeComponent();

            if (!int.TryParse(ConfigurationManager.AppSettings["CaptureDeviceIndex"], out int cdi) ||
                !int.TryParse(ConfigurationManager.AppSettings["MaxChannels"], out int maxChannels))
            {
                throw new ArgumentException("Invalid configuration values for CaptureDeviceIndex or MaxChannels.");
            }

            _fhapi = new FHAPILib.FHAPI();

            string filter = $"src host {ConfigurationManager.AppSettings["BiopacIpAddr"]} or src host {ConfigurationManager.AppSettings["EmotiveIpAddr"]} and udp"; //ADD FILTERING FOR +10 UDP TO SKIP COUNTERS

            _fhapi.SetDeviceIndex(cdi);
            _fhapi.SetFilter(filter);
            _fhapi.StartCapturing();

            _vm = new MonitorWindowViewModel(graphs);
            DataContext = _vm;

            UpdateData();

            this.Closing += (e, s) => { _fhapi.StopCapturing(); };
        }

        public void UpdateData()
        {
            int nChannels = _vm.Graphs.Count;

            _updateTimer.Tick += (s, e) =>
            {
                foreach (var graph in _vm.Graphs)
                {
                    if (graph.Streamer.HasNewData) { graph.PlotControl.Refresh(); }
                }
            };

            _dataTimer.Tick += (s, e) =>
            {
                var packets = _fhapi.GetPackets();

                foreach (var packet in packets)
                {
    
                    int payloadElementSize = packet.PayloadElementSize;
                    int nRepetitions = ((packet.PayloadLength - 2) / payloadElementSize) / nChannels;

                    for (int i = 0; i < nRepetitions * nChannels; i++)
                    {
                        int offset = 1 + (i * payloadElementSize);


                        if (value == null) { continue; }

                        if (!points.TryGetValue(i % nChannels, out var existingList))
                        {
                            existingList = new List<(DateTime, short?)>();
                            points[i % nChannels] = existingList;
                        }

                        existingList.Add((packet.Timestamp, value));
                    }
                }

                if (points.Count > 0) 
                {
                    for (int i = 0; i < _vm.Graphs.Count; i++)
                    {
                        var values = points[i % nChannels].Select(x => (double)(x.Item2 ?? 0)).ToList();
                        _vm.Graphs.ElementAt(i).Streamer.AddRange(values);
                    }
                }
            };
        }
    }
}
