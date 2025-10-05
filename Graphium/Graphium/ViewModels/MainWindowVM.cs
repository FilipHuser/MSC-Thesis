using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Configuration;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using DataHub;
using DataHub.Modules;
using Graphium.Controls;
using Graphium.Core;
using Graphium.Models;
using Graphium.Views;
using Microsoft.VisualBasic;

namespace Graphium.ViewModels
{
    internal class MainWindowViewModel : ViewModelBase
    {
        #region PROPERTIES
        private Hub _dh;
        private MeasurementTabVM? _currentTab;
        public MeasurementTabVM? CurrentTab { get => _currentTab; set => SetProperty(ref _currentTab, value); }
        public ObservableCollection<MeasurementTabVM> MeasurementTabs { get; set; } = [];
        #endregion
        #region RELAY_COMMANDS
        public RelayCommand NewMeasurementTabCmd => new RelayCommand(execute => NewMeasurementTab());
        public RelayCommand CloseMeasurementTabCmd => new RelayCommand((item) => CloseMeasurementTab(item));
        public RelayCommand OpenDataAcquisitionConfigWindowCmd => new RelayCommand(execute => OpenDataAcquisitionConfigWindow());
        #endregion
        public MainWindowViewModel(Window window) : base(window)
        {
            _dh = new DataHub.Hub();
            var getAppSetting = (string key) => ConfigurationManager.AppSettings[key];

            int.TryParse(getAppSetting("CaptureDeviceIndex"), out int captureDeviceIndex);
            int.TryParse(getAppSetting("PayloadSize"), out int payloadSize);
            var ipAddr = getAppSetting("IPAddr");

            string filter = $"udp and src host {ipAddr} and udp[4:2] > {payloadSize}"; // 8 + actuall size
             
            var packetModule = new BiopacSourceModule(captureDeviceIndex, filter, 5);
            var httpModule = new VRSourceModule(getAppSetting("URI"));

            _dh.AddModule(packetModule);
            _dh.AddModule(httpModule);

            NewMeasurementTab();
        }
        #region METHODS
        private void NewMeasurementTab()
        {
            CurrentTab = new MeasurementTabVM(Window , $"Untitled{MeasurementTabs.Count + 1}", ref _dh);
            CurrentTab.MeasurementStartRequested += () => {
                MeasurementTabs.ToList().ForEach(x => x.StopMeasurement());
            };
            MeasurementTabs.Add(CurrentTab);
        }
        private void CloseMeasurementTab(object item)
        {
            if(item is MeasurementTabVM tab) {
                _dh.StopCapturing();
                MeasurementTabs.Remove(tab);
                CurrentTab = null;
            }
        }
        private void OpenDataAcquisitionConfigWindow()
        {
            if (CurrentTab == null)
            {
                MessageBox.Show(
                    "No active measurement tab is selected.",
                    "Warning",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            var dacw = new DataAcquisitionWindow(CurrentTab.Signals);

            if (CurrentTab.IsMeasuring)
            {
                dacw.Closed += (s, e) => { CurrentTab.StartMeasurement(); };
                CurrentTab.StopMeasurement();
            }

            if (dacw.GetViewModel() is DataAcquisitionWindowVM vm)
            {
                vm.SignalConfigCloseRequested += (signals) =>
                {
                    CurrentTab.Signals = new ObservableCollection<SignalBase>(signals);
                };
            }
            dacw.Owner = Window;
            dacw.ShowDialog();
        }
        #endregion
    }
}
