using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Threading.Channels;
using System.Windows.Threading;
using FHAPILib;
using FHMA.Models;
using PacketDotNet;

namespace FHMA.ViewModels
{
    class MonitorWindowViewModel : BaseViewModel
    {
        public ObservableCollection<BiometricSignal> BiometricSignals { get; set; } = new ObservableCollection<BiometricSignal>();

        public MonitorWindowViewModel(ObservableCollection<BiometricSignal> biometricSignals)
        {
            BiometricSignals = biometricSignals;
        }
    }
}
