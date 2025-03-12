using System.Collections.ObjectModel;
using System.Windows.Threading;
using FHAPILib;
using FHMA.Models;

namespace FHMA.ViewModels
{
    class MonitorWindowViewModel
    {
        private readonly FHAPILib.FHAPI _fhapi;
        public ObservableCollection<Graph> Graphs { get; set; } = new ObservableCollection<Graph>();
        public MonitorWindowViewModel(ObservableCollection<Graph> graphs , FHAPILib.FHAPI fhapi)
        {
            _fhapi = fhapi;
            Graphs = graphs;
            _ = UpdateData();
        }

        public async Task UpdateData()
        {
            while (true)
            {
                var packets = _fhapi.GetPackets();
                foreach (var graph in Graphs)
                {
                    graph.Update(packets);
                }
                await Task.Delay(10);
            }
        }
    }
}
