using System.Collections.ObjectModel;
using System.Windows;
using Graphium.Core;
using Graphium.Models;
using Graphium.Views;

namespace Graphium.ViewModels
{
    internal class BSConfigWindowViewModel : BaseViewModel
    {
        #region PROPERTIES
        private BiologicalSignal? _biologicalSignal;
        private ObservableCollection<BiologicalSignal> _biologicalSignals = new ObservableCollection<BiologicalSignal>();
        public ObservableCollection<BiologicalSignal> BiologicalSignals { get => _biologicalSignals; set => SetProperty(ref _biologicalSignals, value); } 
        public BiologicalSignal? BiologicalSignal { get => _biologicalSignal; set => SetProperty(ref _biologicalSignal, value); }
        #endregion
        #region RELAY_COMMANDS
        public RelayCommand AddBSCommand => new RelayCommand(execute => AddBiologicalSignal() , canExecute => BiologicalSignal != null);
        public RelayCommand CreateBSCommand => new RelayCommand(execute => CreateBiologicalSignal());
        #endregion
        public BSConfigWindowViewModel(Window window) : base(window) => Refresh();
        #region METHODS
        private void AddBiologicalSignal() => ((BSConfigWindow)Window).RaiseOnGraphAdded(BiologicalSignal!);
        private void CreateBiologicalSignal()
        {
            var bscw = new BSCreateWindow();
            bscw.Owner = Window;
            bscw.Closed += (e, s) => { Refresh(); };
            bscw.Show();
        }
        private void Refresh()
        {
            BiologicalSignals = new ObservableCollection<BiologicalSignal>(XmlManager.Load<BiologicalSignal>("BiologicalSignals"));
            if (BiologicalSignals.Any()) { BiologicalSignal = BiologicalSignals.First(); }
        }
        #endregion
    }
}
