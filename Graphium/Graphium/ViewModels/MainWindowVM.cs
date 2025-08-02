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
        private Hub _hub;
        private MeasurementTabControlVM? _currentTab;
        public MeasurementTabControlVM? CurrentTab { get => _currentTab; set => SetProperty(ref _currentTab, value); }
        public ObservableCollection<MeasurementTabControlVM> MeasurementTabs { get; set; } = [];
        #endregion
        #region RELAY_COMMANDS
        public RelayCommand NewMeasurementTabCmd => new RelayCommand(execute => NewMeasurementTab());
        public RelayCommand CloseMeasurementTabCmd => new RelayCommand((item) => CloseMeasurementTab(item));
        public RelayCommand OpenDataAcquisitionConfigWindowCmd => new RelayCommand(execute => OpenDataAcquisitionConfigWindow());
        #endregion
        public MainWindowViewModel(Window window) : base(window)
        {
            _hub = new DataHub.Hub();
            var getAppSetting = (string key) => ConfigurationManager.AppSettings[key];

            int.TryParse(getAppSetting("CaptureDeviceIndex"), out int captureDeviceIndex);
            int.TryParse(getAppSetting("PayloadSize"), out int payloadSize);
            var ipAddr = getAppSetting("IPAddr");

            string filter = $"udp and src host {ipAddr} and udp[4:2] > {payloadSize}"; // 8 + actuall size

            var packetModule = new PacketModule(captureDeviceIndex, filter, 5);
            var httpModule = new HTTPModule<string>(getAppSetting("URI"));

            _hub.AddModule(packetModule);
            _hub.AddModule(httpModule);


            MeasurementTabs.Add(new MeasurementTabControlVM(Window , "Untitled.gph" , ref _hub));
            CurrentTab = MeasurementTabs.First();
        }
        #region METHODS
        private void NewMeasurementTab()
        {
            CurrentTab = new MeasurementTabControlVM(Window , $"Untitled{MeasurementTabs.Count + 1}.gph", ref _hub);
            MeasurementTabs.Add(CurrentTab);
        }
        private void CloseMeasurementTab(object item)
        {
            if(item is MeasurementTabControlVM tab) { MeasurementTabs.Remove(tab); }
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
            var dacw = new DataAcquisitionConfigWindow();

            if (dacw.GetViewModel() is DataAcquisitionConfigWindowVM vm)
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
