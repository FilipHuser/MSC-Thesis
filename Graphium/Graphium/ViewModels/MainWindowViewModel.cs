using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Graphium.Controls;
using Graphium.Core;

namespace Graphium.ViewModels
{
    internal class MainWindowViewModel : BaseViewModel
    {
        #region PROPERTIES
        public ObservableCollection<MeasurementTabControlViewModel> MeasurementTabs { get; set; } = [];
        #endregion
        #region RELAY_COMMANDS
        public RelayCommand NewMeasurementTabCmd => new RelayCommand(execute => NewMeasurementTab());
        public RelayCommand CloseMeasurementTabCmd => new RelayCommand((item) => CloseMeasurementTab(item));
        #endregion
        public MainWindowViewModel(Window window) : base(window)
        {
            MeasurementTabs.Add(new MeasurementTabControlViewModel(window , "Untitled.gra"));
        }
        #region METHODS
        private void NewMeasurementTab() => MeasurementTabs.Add(new MeasurementTabControlViewModel(base.Window,$"Untitled{MeasurementTabs.Count + 1}.gph"));
        private void CloseMeasurementTab(object item)
        {
            if(item is MeasurementTabControlViewModel tab)
            {
                MeasurementTabs.Remove(tab);
            }
        }
        #endregion
    }
}
