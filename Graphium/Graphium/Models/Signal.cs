using System.Data;
using System.IO;
using Graphium.Core;
using ScottPlot;
using ScottPlot.AxisPanels;
using ScottPlot.Plottables;

namespace Graphium.Models
{
    internal class Signal : SignalBase
    {
        #region PROPERTIES
        public override int Count => 1;
        public PlotProperties Properties { get; set; }
        public Plot Plot { get; set; } = new Plot();
        public DataLogger? Logger { get; set; }
        #endregion
        #region METHODS
        public Signal(Type source, PlotProperties? properties = null) : base(source)
        {
            Properties = properties ?? new PlotProperties();
            Logger = Plot.Add.DataLogger();
            Logger.ViewSlide(Properties.Capacity);
            Logger.LegendText = this.ToString();
        }
        public override void Update(Dictionary<int, List<object>> data)
        {
            var values = data.First().Value;

            for (int i = 0; i < values.Count; i++)
            {
                Logger?.Add(Convert.ToDouble(values[i]));
            }
        }
        public override string ToString() => Properties.Label??"~";
        #endregion
    }
}
