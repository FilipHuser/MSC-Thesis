using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Serialization;
using FHMA.Core;
using FHMA.Models;
using FHMA.ViewModels;
using FHMA.Views;

namespace FHMA
{
    public partial class MainWindow : Window
    {
        private readonly MainWindowViewModel _vm;
        public MainWindow()
        {
            InitializeComponent();
            _vm = new MainWindowViewModel(this);
            DataContext = _vm;
        }

        private void Button_StartCapturing(object sender, RoutedEventArgs e)
        {
            if (_vm.BiometricSignals.Count == 0)
            {
                MessageBox.Show("You need to add at least one graph before proceeding.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var mw = new MonitorWindow(_vm.BiometricSignals);
            mw.Show();
            this.Close();
        }
        private void Button_SaveConfiguration(object sender, RoutedEventArgs e)
        {
            XmlManager.Store("BiometricSignalsConfiguration" , "BSConf.xml" , _vm.BiometricSignals.ToList() , true);
        }
    }
}