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
        public List<PlotProperties> AllPlotProperties => Signals.Select(x => x.Properties).ToList();
        public List<Plot> Plots => Signals.Select(x => x.Plot).ToList();
        public List<DataLogger?> Loggers => Signals.Select(x => x.Logger).ToList();
        #endregion
        #region METHODS

        public SignalComposite(Type type) : base(type) { }
        public SignalComposite(Type type, List<PlotProperties> graphs, string name) : base(type)
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
                    var singleData = new Dictionary<int, List<object>> { { index, kvp.Value } };

                    Signals[index].Update(singleData);
                }
            }
        }

        public override string ToString() => Name ?? string.Join(",", AllPlotProperties.Select(x => x.Label));
        #endregion
    }
}
