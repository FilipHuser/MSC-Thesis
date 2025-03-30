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
            _vm = new MainWindowViewModel();
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
        private void Button_LoadConfiguration(object sender, RoutedEventArgs e)
        {
            _vm.Refresh();
        }
        private void Button_AddBiometricSignal(object sender, RoutedEventArgs e)
        {
            var bscw = new BiometricSignalConfigWindow(this);
            bscw.OnGraphAdded += (biometricSignal) => {
                if (_vm.BiometricSignals.Count == _vm.MaxChannels)
                {
                    MessageBox.Show("You have reached the maximum number of channels allowed.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                _vm.BiometricSignals.Add(biometricSignal);
            };
            bscw.Owner = this;
            bscw.ShowDialog();
        }
        private void Button_RemoveGraph(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is BiometricSignal biometricSignal) { _vm.BiometricSignals.Remove(biometricSignal); }
        }
    }
}