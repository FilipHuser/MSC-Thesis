using System.Text.Json.Serialization;
using DataHub.Core;

namespace Graphium.Models
{
    public class Signal : SignalBase
    {
        #region PROPERTIES
        [JsonIgnore] public List<double> X { get; set; } = [];
        [JsonIgnore] public List<double> Y { get; set; } = [];
        public PlotProperties Properties { get; set; } = new();
        #endregion
        #region METHODS
        public Signal() : base(string.Empty , ModuleType.NONE)
        {
            
        }
        public Signal(string name, ModuleType source, PlotProperties? properties = null) : base(name , source)
        {
            Properties = properties??new PlotProperties();
            Init();
        }
        private void Init()
        { 
        }
        public override void Update(Dictionary<int, List<object>> data)
        {
        }
        public override IEnumerable<Signal> GetSignals()
        {
            yield return this;
        }
        #endregion
    }
}
