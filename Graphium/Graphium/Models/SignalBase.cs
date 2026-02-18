using DataHub.Core;
using Graphium.Enums;
using System.Text.Json.Serialization;

namespace Graphium.Models
{
    [JsonDerivedType(typeof(TextSignal), "text")]
    [JsonDerivedType(typeof(NumericSignal), "numeric")]
    public abstract class SignalBase
    {
        #region PROPERTIES
        public string Name { get; set; }
        public SignalType Type { get; set; }
        public ModuleType Source { get; set; }
        public bool IsPlotted { get; set; } = true;
        [JsonIgnore] public bool IsAcquired { get; set; } = true;
        [JsonIgnore] public List<double> XData { get; set; } = new();
        #endregion
        #region METHODS
        protected SignalBase()
        {
            Name = string.Empty;
            Source = ModuleType.NaN;
        }
        protected SignalBase(string name, SignalType type, ModuleType source)
        {
            Name = name;
            Type = type;
            Source = source;
        }
        public abstract void Update(double absoluteTimeSeconds, object? data);
        public virtual void ClearData() => XData.Clear();
        public override bool Equals(object? obj)
        {
            if (obj is not SignalBase other) return false;
            return Name == other.Name && Source == other.Source;
        }
        public override int GetHashCode() => HashCode.Combine(Name, Source);
        #endregion
    }
}