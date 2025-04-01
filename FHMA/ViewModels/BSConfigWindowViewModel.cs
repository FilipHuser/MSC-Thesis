using System.Collections.ObjectModel;
using System.Windows;
using FHMA.Core;
using FHMA.Models;
using FHMA.Views;

namespace FHMA.ViewModels
{
    internal class BSConfigWindowViewModel : BaseViewModel
    {
        #region PROPERTIES
        private BiometricSignal? _biometricSignal;
        private ObservableCollection<BiometricSignal> _biometricSignals = new ObservableCollection<BiometricSignal>();
        public ObservableCollection<BiometricSignal> BiometricSignals { get => _biometricSignals; set => SetProperty(ref _biometricSignals, value); } 
        public BiometricSignal? BiometricSignal { get => _biometricSignal; set => SetProperty(ref _biometricSignal, value); }
        #endregion
        #region RELAY_COMMANDS
        public RelayCommand AddBSCommand => new RelayCommand(execute => AddBiometricSignal() , canExecute => BiometricSignal != null);
        public RelayCommand CreateBSCommand => new RelayCommand(execute => CreateBiometricSignal());
        #endregion
        public BSConfigWindowViewModel(Window window) : base(window) => Refresh();
        #region METHODS
        private void AddBiometricSignal() => ((BSConfigWindow)Window).RaiseOnGraphAdded(BiometricSignal!);
        private void CreateBiometricSignal()
        {
            var bscw = new BSCreateWindow();
            bscw.Owner = Window;
            bscw.Closed += (e, s) => { Refresh(); };
            bscw.Show();
        }
        private void Refresh()
        {
            BiometricSignals = new ObservableCollection<BiometricSignal>(XmlManager.Load<BiometricSignal>("BiometricSignals"));
            if (BiometricSignals.Any()) { BiometricSignal = BiometricSignals.First(); }
        }
        #endregion
    }
}
