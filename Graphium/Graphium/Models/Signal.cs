using System.Data;
using System.IO;
using Graphium.Core;
using ScottPlot;
using ScottPlot.AxisPanels;
using ScottPlot.Plottables;

namespace Graphium.Models
{
    public class Signal : SignalBase
    {
        #region PROPERTIES
        public override int Count => 1;
        public PlotProperties Properties { get; set; }
        public Plot Plot { get; set; } = new Plot();
        public DataStreamer? Streamer { get; set; }
        public override string? Name { get => Properties.Label; set => Properties.Label = value; }
        #endregion
        #region METHODS
        public Signal(Type source, PlotProperties? properties = null) : base(source)
        {
            Properties = properties ?? new PlotProperties();
            Streamer = Plot.Add.DataStreamer(Properties.Capacity);
            Streamer.LegendText = this.ToString();
            //Streamer.ManageAxisLimits = true;
            Plot.Axes.SetLimitsY(Properties.LowerBound , Properties.UpperBound);
            //Plot.Axes.Bottom.TickLabelStyle.IsVisible = false;
            Plot.Axes.Left.TickLabelStyle.IsVisible = false;
        }
        public override void Update(Dictionary<int, List<object>> data)
        {
            var values = data.First().Value;
            Streamer?.AddRange(values.Select(x => Convert.ToDouble(x)));
        }
        public override string ToString() => Properties.Label??"~";
        #endregion
    }
}
