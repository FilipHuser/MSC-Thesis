using System.Collections.ObjectModel;
using System.Configuration;
using System.Windows;
using FHMA.Models;
using static FHAPI.Core.FHPacket;

namespace FHMA.ViewModels
{
    #region AVAILABLE_ITEMS

    #endregion
    internal class BSCreateWindowViewModel : BaseViewModel
    {
        #region AVAILABLE_ITEMS
        public List<PacketSource> Sources { get; }
        #endregion
        #region SELECTED_ITEMS
        public BiometricSignal BiometricSignal { get; set; } = new BiometricSignal();
        public ObservableCollection<Graph> Graphs { get; } = new ObservableCollection<Graph>();
        private Graph _graph = new Graph();
        public Graph Graph { get => _graph; set => SetProperty(ref _graph, value); }
        #endregion

        public BSCreateWindowViewModel(Window window) : base(window)
        {
            Sources = Enum.GetValues(typeof(PacketSource)).Cast<PacketSource>().ToList();
            BiometricSignal.Source = Sources.First();
        }
    }
}
