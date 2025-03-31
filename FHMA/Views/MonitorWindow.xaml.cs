using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using FHAPILib;
using FHMA.Core;
using FHMA.Models;
using FHMA.ViewModels;
using ScottPlot.WPF;
using SkiaSharp;

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

        public MonitorWindow(ObservableCollection<BiometricSignal> biometricSignals)
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

            _vm = new MonitorWindowViewModel(this , biometricSignals);
            DataContext = _vm;

            UpdateData();

            this.Closing += (e, s) => { _fhapi.StopCapturing(); };
        }

        public void UpdateData()
        {
            var biometricSignalsBySource = _vm.BiometricSignals.ToList().GroupBy(x => x.Source);

            _updateTimer.Tick += (s, e) =>
            {
                foreach (var graph in _vm.BiometricSignals.SelectMany(x => x.Graphs))
                { 
                   if (graph.Streamer.HasNewData) { graph.PlotControl.Refresh(); }
                }
            };

            _dataTimer.Tick += (s, e) =>
            {
                var packets = _fhapi.GetPackets();

                foreach (var packet in packets)
                {
                    if(packet.PayloadLength < 18) { continue; } // AFTER ADJUSTING FILTER NO NEED FOR THIS

                    var matchingGraphs = biometricSignalsBySource.FirstOrDefault(x => x.Key == packet.Source)?.SelectMany(x => x.Graphs).ToList();

                    if (matchingGraphs == null) { continue; }

                    var points = packet.ExtractData(matchingGraphs.Count);

                    if (points == null || points.Count == 0) { continue; }

                    for (int i = 0; i < matchingGraphs.Count; i++)
                    {
                        var values = points[i % matchingGraphs.Count].Select(x => x.Item2);
                        matchingGraphs.ElementAt(i).Streamer.AddRange(values);
                    }
                }
            };
        }
    }
}
