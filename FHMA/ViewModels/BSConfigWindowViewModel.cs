using System.Collections.ObjectModel;
using System.Windows;
using FHMA.Core;
using FHMA.Models;

namespace FHMA.ViewModels
{
    internal class BSConfigWindowViewModel : BaseViewModel
    {
        #region AVAILABLE_ITEMS
        private ObservableCollection<BiometricSignal> _biometricSignals = new ObservableCollection<BiometricSignal>();
        public ObservableCollection<BiometricSignal> BiometricSignals { get => _biometricSignals; set => SetProperty(ref _biometricSignals, value); } 
        #endregion

        #region SELECTED_ITEMS
        private BiometricSignal _biometricSignal = new BiometricSignal();
        public BiometricSignal BiometricSignal { get => _biometricSignal; set => SetProperty(ref _biometricSignal, value); }
        #endregion

        public BSConfigWindowViewModel(Window window) : base(window)
        {
            Refresh();
        }

        public void Refresh()
        {
            BiometricSignals = new ObservableCollection<BiometricSignal>(XmlManager.Load<BiometricSignal>("BiometricSignals"));
            if (BiometricSignals.Any()) { BiometricSignal = BiometricSignals.First(); }
        }
    }
}
