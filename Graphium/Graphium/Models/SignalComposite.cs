using System.Text.Json.Serialization;
using ScottPlot;
using ScottPlot.Plottables;

namespace Graphium.Models
{
    internal class SignalComposite : SignalBase
    {
        #region PROPERTIES
        public override int Count => Signals.Count;
        public override string? Name { get; set; }
        public List<Signal> Signals { get; set; } = new List<Signal>();
        public List<PlotProperties> AllPlotProperties => Signals.Select(x => x.Properties).ToList();
        [JsonIgnore]
        public List<Plot> Plots => Signals.Select(x => x.Plot).ToList();
        [JsonIgnore]
        public List<DataStreamer?> Loggers => Signals.Select(x => x.Streamer).ToList();
        public override List<PlotProperties> PlotProperties => AllPlotProperties;
        #endregion
        #region METHODS
        [JsonConstructor]
        protected SignalComposite() : base(typeof(object)){}
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
