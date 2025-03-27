using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Threading.Channels;
using System.Windows.Threading;
using FHAPILib;
using FHMA.Models;
using PacketDotNet;

namespace FHMA.ViewModels
{
    class MonitorWindowViewModel
    {
        public ObservableCollection<Graph> Graphs { get; set; } = new ObservableCollection<Graph>();
        public MonitorWindowViewModel(ObservableCollection<Graph> graphs)
        {
            Graphs = graphs;
        }
    }
}
