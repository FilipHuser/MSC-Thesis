using System.Data;
using System.IO;
using System.Text.Json.Serialization;
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
        [JsonIgnore]
        public Plot Plot { get; set; } = new Plot();
        [JsonIgnore]
        public DataStreamer? Streamer { get; set; }
        public override string? Name { get => Properties.Label; set => Properties.Label = value; }
        public override List<PlotProperties> PlotProperties => new() { Properties };
        #endregion
        #region METHODS
        [JsonConstructor]
        public Signal(PlotProperties properties, string sourceTypeName, bool isPlotted, bool isAcquired) : base(Type.GetType(sourceTypeName) ?? typeof(object))
        {
            Properties = properties;
            IsPlotted = isPlotted;
            IsAcquired = isAcquired;
            InitializePlot();
        }
        public Signal(Type source, PlotProperties? properties = null) : base(source)
        {
            Properties = properties ?? new PlotProperties();
            InitializePlot();
        }
        private void InitializePlot()
        {
            Streamer = Plot.Add.DataStreamer(Properties.Capacity);
            Streamer.LegendText = this.ToString();
            Streamer.ManageAxisLimits = true;
            Plot.Axes.SetLimitsY(Properties.LowerBound, Properties.UpperBound);
            Plot.Axes.Bottom.TickLabelStyle.IsVisible = false;
            PixelPadding padding = new(50, 20, 30, 5);
            Plot.Layout.Fixed(padding);
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
