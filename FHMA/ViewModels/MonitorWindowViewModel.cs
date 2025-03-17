using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Threading.Channels;
using System.Windows.Threading;
using FHAPILib;
using FHMA.Models;
using LiveChartsCore.Defaults;
using PacketDotNet;

namespace FHMA.ViewModels
{
    class MonitorWindowViewModel
    {
        private readonly int _nRepetitions;
        private readonly FHAPILib.FHAPI _fhapi;
        public ObservableCollection<Graph> Graphs { get; set; } = new ObservableCollection<Graph>();
        public MonitorWindowViewModel(int nRepetitions , ObservableCollection<Graph> graphs , FHAPILib.FHAPI fhapi)
        {
            _nRepetitions = nRepetitions;
            _fhapi = fhapi;
            Graphs = graphs;

            _ = UpdateData();
        }

        public async Task UpdateData()
        {
            int nChannels = Graphs.Count;

            while (true)
            {
                var packets = _fhapi.GetPackets();

                Dictionary<int, List<DateTimePoint>> points = new Dictionary<int, List<DateTimePoint>>();
                foreach (var packet in packets)
                {
                    for (int i = 0; i < _nRepetitions * nChannels; i++)
                    {
                        int offset = 1 + (i * 2);
                        var value = Convertor<short>.ConvertPayload(packet.Payload.Skip(offset).Take(2).ToArray(), 0);

                        if (value == null) { continue; }

                        if (!points.TryGetValue(i % nChannels, out var channelPoints))
                        {
                            points[i % nChannels] = new List<DateTimePoint>();
                        }

                        points[i % nChannels].Add(new DateTimePoint(packet.Timestamp, value));
                    }
                }

                if (points.Count == 0) { await Task.Delay(100); continue; }
                foreach (var graph in Graphs)
                {
                    var graphPoints = points[graph.Channel % nChannels];
                    graph.Update(graphPoints);
                }


                await Task.Delay(100);
            }
        }
    }
}
