using System.IO;
using ScottPlot;
using ScottPlot.Plottables;

namespace Graphium.Models
{
    internal class Signal : SignalBase
    {
        #region PROPERTIES
        public override int Count => 1;
        public Graph Graph { get; set; }
        public Plot Plot { get; set; } = new Plot();
        public DataStreamer Stream { get; set; }
        #endregion
        #region METHODS
        public Signal(Type source, Graph? graph = null) : base(source)
        {
            Graph = graph ?? new Graph();
            //Stream = Plot.Add.DataStreamer(Graph.Capacity);
            //Stream.LegendText = Graph.Label ?? "~";
        }
        public override void Update(Dictionary<int, List<object>> data) => Stream.AddRange(data.Single().Value.Select(v => Convert.ToDouble(v)));
        public override string ToString() => Graph.Label??"~";
        #endregion
    }
}
