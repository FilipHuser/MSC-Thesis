using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using DataHub.Modules;
using Graphium.Controls;
using Graphium.Core;
using Graphium.Interfaces;
using Graphium.Models;
using Graphium.Views;

namespace Graphium.ViewModels
{
    class SignalConfigControlVM : ViewModelBase, IMenuItemViewModel
    {
        #region PROPERTIES
        public string Header => "Signals";
        public UserControl Content { get; }
        public ObservableCollection<SignalBase> Signals { get; set; } = new ObservableCollection<SignalBase>();

        public delegate void CloseEventHandler(List<SignalBase> signals);
        public event CloseEventHandler? CloseRequested;
        #endregion
        #region RELAY_COMMANDS
        public RelayCommand CloseCmd => new RelayCommand(execute => Close());
        #endregion

        public SignalConfigControlVM(Window parent) : base(parent)  
        {
            Content = new SignalsConfigControl(parent)
            {
                DataContext = this
            };

            var respSignal = new Signal(typeof(PacketModule), new Graph() { Label = "RESP", Capacity = 10000 });
            var ecgSignal = new Signal(typeof(PacketModule), new Graph() { Label = "ECG", Capacity = 10000, LowerBound = -0.5, UpperBound = 0.5 });
            var spiderCountSignal = new Signal(typeof(HTTPModule<string>), new Graph() { Label = "Number of Spiders" });
            var spiderSizeSignal = new Signal(typeof(HTTPModule<string>), new Graph() { Label = "size" });


            var rsprCompound = new SignalComposite(typeof(PacketModule), new() { respSignal.Graph, ecgSignal.Graph }, "RSP-R");
            var vrDataCompound = new SignalComposite(typeof(HTTPModule<string>), new() { spiderCountSignal.Graph, spiderSizeSignal.Graph }, "VR Data - Spiders");

            Signals.Add(respSignal);
            Signals.Add(ecgSignal);
            Signals.Add(vrDataCompound);
            Signals.Add(rsprCompound);
        }
        #region METHODS
        private void Close()
        {
            CloseRequested?.Invoke(Signals.Where(x => x.IsAcquired).ToList());
            Window.Close();
        }
        #endregion
    }
}
