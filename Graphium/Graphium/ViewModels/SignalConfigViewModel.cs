using System.Collections.ObjectModel;
using System.Windows;
using DataHub.Modules;
using Graphium.Core;
using Graphium.Models;
using Graphium.Views;

namespace Graphium.ViewModels
{
    internal class SignalConfigViewModel : BaseViewModel
    {
        #region PROPERTIES
        private SignalBase? _signal;
        private ObservableCollection<SignalBase> _signals = [];
        public ObservableCollection<SignalBase> Signals { get => _signals; set => SetProperty(ref _signals, value); }
        public SignalBase? Signal { get => _signal; set => SetProperty(ref _signal, value); }
        #endregion
        #region RELAY_COMMANDS
        public RelayCommand AddSignalCommand => new RelayCommand(execute => addSignal(), canExecute => Signal != null);
        public RelayCommand CreateSignalCommand => new RelayCommand(execute => createSignal(), canExecute => false);
        #endregion
        #region METHODS
        public SignalConfigViewModel(Window window) : base(window)
        {
            var respSignal = new Signal(typeof(PacketModule), new Graph() { Label = "RESP" , Capacity = 10000 });
            var ecgSignal = new Signal(typeof(PacketModule), new Graph() { Label = "ECG" , Capacity = 2500 , LowerBound = -2.5 , UpperBound = 2.5 });
            var spiderCountSignal = new Signal(typeof(HTTPModule<string>), new Graph() { Label = "Number of Spiders" });
            var spiderSizeSignal = new Signal(typeof(HTTPModule<string>), new Graph() { Label = "size" });


            var rsprCompound = new SignalComposite(typeof(PacketModule) , new(){ respSignal , ecgSignal } , "RSP-R");
            var vrDataCompound = new SignalComposite(typeof(HTTPModule<string>), new() { spiderCountSignal, spiderSizeSignal }, "VR Data - Spiders");

            Signals.Add(respSignal);
            Signals.Add(ecgSignal);
            Signals.Add(vrDataCompound);
            Signals.Add(rsprCompound);

            Signal = Signals.First();
        }
        private void addSignal() => ((SignalConfigWindow)Window).RaiseOnGrapAdded(Signal!);
        private void createSignal() { }
        #endregion
    }
}
