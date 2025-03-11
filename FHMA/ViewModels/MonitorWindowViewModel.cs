using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using FHAPILib;
using FHMA.Models;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace FHMA.ViewModels
{
    class MonitorWindowViewModel
    {
        private readonly FHAPILib.FHAPI _fhapi;
        public ObservableCollection<Graph> Graphs { get; set; } = new ObservableCollection<Graph>();
        public CancellationTokenSource CTS { get; set; } = new CancellationTokenSource();
        public MonitorWindowViewModel(ObservableCollection<Graph> graphs , FHAPILib.FHAPI fhapi)
        {
            _fhapi = fhapi;
            Graphs = graphs;
            Task.Run(async () => await UpdateData(CTS.Token));
        }

        public async Task UpdateData(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var packets = _fhapi.GetPackets();
                var tasks = new List<Task>();

                if (packets.Count == 0) { continue; }

                foreach (var graph in Graphs)
                {
                    tasks.Add(graph.Update(packets));
                }
                await Task.WhenAll(tasks);
            }
        }
    }
}
