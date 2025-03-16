using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using FHMA.Models;
using FHMA.ViewModels;

namespace FHMA.Views
{
    /// <summary>
    /// Interaction logic for MonitorWindow.xaml
    /// </summary>
    public partial class MonitorWindow : Window
    {
        private readonly FHAPILib.FHAPI _fhapi;
        private readonly MonitorWindowViewModel _vm;
        public MonitorWindow(ObservableCollection<Graph> graphs)
        {
            InitializeComponent();

            if (!int.TryParse(ConfigurationManager.AppSettings["CaptureDeviceIndex"], out int cdi) ||
                !int.TryParse(ConfigurationManager.AppSettings["MaxChannels"], out int maxChannels))
            {
                throw new ArgumentException("Invalid configuration values for CaptureDeviceIndex or MaxChannels.");
            }

            _fhapi = new FHAPILib.FHAPI();
            
            int count = graphs.Count;
            int nRepetitions = (int)Math.Ceiling((maxChannels / 2.0) / count);

            if (nRepetitions == 1 && count < maxChannels) { nRepetitions++; }

            int payloadLength = (2 * count * nRepetitions) + 2;

            string filter = $"src host {ConfigurationManager.AppSettings["DeviceIpAddr"]} and udp and udp[4:2] = {payloadLength}";



            _fhapi.SetDeviceIndex(cdi);
            _fhapi.SetFilter(filter);
            _fhapi.StartCapturing();

            _vm = new MonitorWindowViewModel(graphs , _fhapi);
            DataContext = _vm;

            this.Closing += (e, s) => { _fhapi.StopCapturing(); };
        }
    }
}
