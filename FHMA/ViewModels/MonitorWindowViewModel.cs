using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows.Threading;
using FHAPILib;
using FHMA.Models;
using LiveChartsCore.Defaults;
using PacketDotNet;

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


                Dictionary<int , List<DateTimePoint>> points = new Dictionary<int, List<DateTimePoint>>();

                foreach (var packet in packets)
                {
                    var payload = packet.Payload;
                    int nChannels = Graphs.Count;


                    //int nRepetitions = 1 << (int)Math.Floor(Math.Log2(nChannels));








                    /*
                    // First value: Take 2 bytes after skipping 'offset'
                    var firstValue = Convertor<short>.ConvertPayload(packet.Payload.Skip(offset).Take(2).ToArray(), 0) ?? 0;

                    // Second value: Take another 2 bytes after skipping the offset for the second value
                    var secondValue = Convertor<short>.ConvertPayload(packet.Payload.Skip(offset + 4).Take(2).ToArray(), 0) ?? 0;

                    // Add both values to DateTimePoints list (you may want to map both values to DateTimePoint)
                    points.Add(new DateTimePoint(packet.Timestamp, firstValue));
                    points.Add(new DateTimePoint(packet.Timestamp, secondValue));
                    */
                }




                foreach (var graph in Graphs)
                {
                    //graph.Update();
                }
                await Task.Delay(10);
            }
        }
    }
}
