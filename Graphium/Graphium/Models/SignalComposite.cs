using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using DataHub.Core;
using DataHub.Interfaces;
using ScottPlot;
using ScottPlot.Plottables;

namespace Graphium.Models
{
    internal class SignalComposite : SignalBase
    {
        #region PROPERTIES
        private List<Signal> _signals = []; 
        public List<Signal> Signals 
        { 
            get => _signals;
            set
            {
                if (value != null && value.Any(s => s.Source != Source))
                    throw new InvalidOperationException("All signals must have the same ModuleType as the composite.");
                _signals = value ?? new List<Signal>();
            }
        }
        #endregion
        #region METHODS
        public SignalComposite(string name , ModuleType source) : base(name, source) { }
        public SignalComposite(string name, List<Signal> signals) : base(name, GetModuleType(signals))
        {
            Signals = signals;
        }
        private static ModuleType GetModuleType(List<Signal> signals)
        {
            if (signals == null || signals.Count == 0) { throw new ArgumentException("Signals cannot be null or empty"); }
            return signals[0].Source;
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
        public override IEnumerable<Signal> GetSignals() => Signals;
        #endregion
    }
}
