using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Graphium.Controls;
using Graphium.Core;
using Graphium.Views;
using Microsoft.VisualBasic;

namespace Graphium.ViewModels
{
    internal class MainWindowViewModel : BaseViewModel
    {
        #region PROPERTIES
        private MeasurementTabControlViewModel? _selectedTab;
        public MeasurementTabControlViewModel? SelectedTab { get => _selectedTab; set => SetProperty(ref _selectedTab, value); }
        public ObservableCollection<MeasurementTabControlViewModel> MeasurementTabs { get; set; } = [];
        #endregion
        #region RELAY_COMMANDS
        public RelayCommand NewMeasurementTabCmd => new RelayCommand(execute => NewMeasurementTab());
        public RelayCommand CloseMeasurementTabCmd => new RelayCommand((item) => CloseMeasurementTab(item));
        public RelayCommand OpenSingalConfigWindowCmd => new RelayCommand(execute => OpenSignalConfigWindow());
        #endregion
        public MainWindowViewModel(Window window) : base(window)
        {
            MeasurementTabs.Add(new MeasurementTabControlViewModel(window , "Untitled.gph"));
            SelectedTab = MeasurementTabs.First();
        }
        #region METHODS
        private void NewMeasurementTab()
        {
            SelectedTab = new MeasurementTabControlViewModel(base.Window, $"Untitled{MeasurementTabs.Count + 1}.gph");
            MeasurementTabs.Add(SelectedTab);
        }
        private void CloseMeasurementTab(object item)
        {
            if(item is MeasurementTabControlViewModel tab)
            {
                MeasurementTabs.Remove(tab);
            }
        }
        private void OpenSignalConfigWindow()
        {
            SignalConfigWindow scw = new SignalConfigWindow();
            scw.Owner = Window;
            scw.ShowDialog();
        }
        #endregion
    }
}
