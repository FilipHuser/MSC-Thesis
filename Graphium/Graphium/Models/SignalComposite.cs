using DataHub.Core;

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
                if (value == null)
                {
                    _signals = new List<Signal>();
                    return;
                }

                if (Source == ModuleType.NONE && value.Count > 0) { Source = value[0].Source; }

                if (value.Any(s => s.Source != Source))
                    throw new InvalidOperationException("All signals must have the same ModuleType as the composite.");

                _signals = value;
            }
        }
        #endregion
        #region METHODS
        public SignalComposite() : base(string.Empty, ModuleType.NONE) { }
        public SignalComposite(string name, List<Signal> signals) : base(name, GetModuleType(signals))
        {
            Signals = signals;
        }
        public void Add(Signal signal) => Signals.Add(signal);
        public void Remove(Signal signal) => Signals?.Remove(signal);
        public override void Update(Dictionary<int, List<object>> data, double elapsedTime)
        {
            foreach (var kvp in data)
            {
                int index = kvp.Key;
                if (index >= 0 && index < Signals.Count)
                {
                    var singleData = new Dictionary<int, List<object>> { { index, kvp.Value } };
                    Signals[index].Update(singleData, elapsedTime);
                }
            }
        }
        public override IEnumerable<Signal> GetSignals() => Signals;
        private static ModuleType GetModuleType(List<Signal> signals)
        {
            if (signals == null || signals.Count == 0) { throw new ArgumentException("Signals cannot be null or empty"); }
            return signals[0].Source;
        }
        #endregion
    }
}
