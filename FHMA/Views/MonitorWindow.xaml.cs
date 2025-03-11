using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Text;
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
            int.TryParse(ConfigurationManager.AppSettings["CaptureDeviceIndex"], out int cdi);

            _fhapi = new FHAPILib.FHAPI();
            _fhapi.SetDeviceIndex(cdi);
            _fhapi.SetFilter(ConfigurationManager.AppSettings["Filter"]??"");
            _fhapi.StartCapturing();

            _vm = new MonitorWindowViewModel(graphs , _fhapi);
            DataContext = _vm;

            this.Closing += (e, s) => { _fhapi.StopCapturing(); };
        }
    }
}
