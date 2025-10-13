using System.Data;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using DataHub.Core;
using DataHub.Interfaces;
using Graphium.Core;
using ScottPlot;
using ScottPlot.AxisPanels;
using ScottPlot.Plottables;
using ScottPlot.WPF;

namespace Graphium.Models
{
    public class Signal : SignalBase
    {
        #region PROPERTIES
        [JsonIgnore]
        public List<double> X { get; set; } = new();
        [JsonIgnore]
        public List<double> Y { get; set; } = new();
        public PlotProperties Properties { get; set; }
        #endregion
        #region METHODS
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
            // Assuming you want to **accumulate** data rather than overwrite
            if (data.TryGetValue(0, out var list))
            {
                foreach (var item in list)
                {
                    double value;
                    if (item is double d)
                        value = d;
                    else if (!double.TryParse(item.ToString(), out value))
                        continue; // skip invalid entries

                    X.Add(value);                     // measurement value on X-axis
                    Y.Add(DateTime.Now.Subtract(DateTime.Today).TotalSeconds); // time in seconds today
                }
            }
        }
        public override IEnumerable<Signal> GetSignals()
        {
            yield return this;
        }
        #endregion
    }
}
