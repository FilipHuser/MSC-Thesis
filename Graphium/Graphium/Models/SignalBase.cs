using DataHub.Core;
using Graphium.Interfaces;
using System.Text.Json.Serialization;

namespace Graphium.Models
{
    [JsonDerivedType(typeof(Signal), "Signal")]
    [JsonDerivedType(typeof(SignalComposite), "SignalComposite")]
    public abstract class SignalBase : ISignalSource
    {
        #region PROPERTIES
        public string Name { get; set; }
        public ModuleType Source { get; set; }
        [JsonIgnore] public bool IsPlotted { get; set; } = true;
        [JsonIgnore] public bool IsAcquired { get; set; } = true;
        #endregion
        #region METHODS
        public SignalBase(string name, ModuleType source)
        {
            Name = name;
            Source = source;
        }
        public abstract void Update(Dictionary<int, List<object>> data, double elapsedTime);
        public abstract IEnumerable<Signal> GetSignals();
        public override bool Equals(object? obj)
        {
            if (obj is not SignalBase other) return false;
            return Name == other.Name && Source == other.Source;
        }
        public override int GetHashCode() => HashCode.Combine(Name, Source);
        #endregion
    }
}
