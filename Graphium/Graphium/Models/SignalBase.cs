using System.Text.Json.Serialization;
using System.Xml.Linq;
using Graphium.Models;

namespace Graphium.Models
{
    [JsonDerivedType(typeof(Signal), "Signal")]
    [JsonDerivedType(typeof(SignalComposite), "SignalComposite")]
    public abstract class SignalBase
    {
        #region PROPERTIES

        public string SourceTypeName { get; set; }
        public abstract string? Name { get; set; }
        public abstract int Count { get; }
        public bool IsPlotted { get; set; }
        public bool IsAcquired { get; set; }
        public abstract List<PlotProperties> PlotProperties { get; }  
        #endregion
        #region METHODS
        public SignalBase(Type source)
        {
            SourceTypeName = source.AssemblyQualifiedName ?? source.FullName ?? source.Name;
        }
        public abstract void Update(Dictionary<int , List<object>> data);
        public Type? GetSourceType() => Type.GetType(SourceTypeName);
        #endregion
    }
}
