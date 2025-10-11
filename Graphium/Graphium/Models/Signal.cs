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
        }
        public override IEnumerable<Signal> GetSignals()
        {
            yield return this;
        }
        #endregion
    }
}
