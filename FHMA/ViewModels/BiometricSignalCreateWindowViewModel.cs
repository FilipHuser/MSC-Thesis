using System.Collections.ObjectModel;
using System.Configuration;
using FHMA.Models;
using static FHAPI.Core.FHPacket;

namespace FHMA.ViewModels
{
    #region AVAILABLE_ITEMS

    #endregion
    internal class BiometricSignalCreateWindowViewModel : BaseViewModel
    {
        #region AVAILABLE_ITEMS
        public List<PacketSource> Sources { get; }
        public List<SignalType> SignalTypes { get; }
        #endregion
        #region SELECTED_ITEMS
        public BiometricSignal BiometricSignal { get; set; } = new BiometricSignal();
        public ObservableCollection<Graph> Graphs { get; } = new ObservableCollection<Graph>();
        private Graph _graph = new Graph();
        public Graph Graph { get => _graph; set => SetProperty(ref _graph, value); }
        #endregion

        public BiometricSignalCreateWindowViewModel()
        {
            Sources = Enum.GetValues(typeof(PacketSource)).Cast<PacketSource>().ToList();
            SignalTypes = Enum.GetValues(typeof(SignalType)).Cast<SignalType>().ToList();   

            BiometricSignal.Source = Sources.First();
            BiometricSignal.Type = SignalTypes.First();
        }
    }
}
