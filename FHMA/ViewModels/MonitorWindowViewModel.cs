using System.Collections.ObjectModel;
using System.Windows;
using FHMA.Models;

namespace FHMA.ViewModels
{
    class MonitorWindowViewModel : BaseViewModel
    {
        public ObservableCollection<BiometricSignal> BiometricSignals { get; set; } = new ObservableCollection<BiometricSignal>();

        public MonitorWindowViewModel(Window window , ObservableCollection<BiometricSignal> biometricSignals) : base(window)
        {
            BiometricSignals = biometricSignals;
        }
    }
}
