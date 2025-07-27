using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using ScottPlot;
using ScottPlot.Plottables;

namespace Graphium.Models
{
    internal class SignalComposite : SignalBase
    {
        #region PROPERTIES
        public override int Count => Signals.Count;
        public string? Name { get; set; }
        public List<Signal> Signals { get; private set; } = new List<Signal>();
        public List<Graph> Graphs => Signals.Select(x => x.Graph).ToList();
        public List<Plot> Plots => Signals.Select(x => x.Plot).ToList();
        public List<DataStreamer> Streams => Signals.Select(x => x.Stream).ToList();
        #endregion
        #region METHODS

        public SignalComposite(Type type) : base(type) { }
        public SignalComposite(Type type, List<Graph> graphs, string name) : base(type)
        {
            Name = name;
            Signals = graphs.Select(graph => new Signal(type, graph)).ToList();
        }
        public void Add(Signal signal) => Signals.Add(signal);
        public void Remove(Signal signal) => Signals?.Remove(signal);

        public override void Update(Dictionary<int, List<object>> data)
        {
            foreach (var kvp in data)
            {
                int index = kvp.Key;
                if (index >= 0 && index < Signals.Count)
                {
                    Signals[index].Stream.AddRange(kvp.Value.Select(v => Convert.ToDouble(v)).ToList());
                }
            }
        }

        public override string ToString() => Name ?? string.Join(",", Graphs.Select(x => x.Label));
        #endregion
    }
}
