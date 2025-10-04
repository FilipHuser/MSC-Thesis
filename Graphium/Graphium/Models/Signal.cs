using System.Data;
using System.IO;
using System.Text.Json.Serialization;
using DataHub.Core;
using DataHub.Interfaces;
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
        public List<DataStreamer?> Streamers { get; set; } = [];
        public override string? Name { get => Properties.Label; set => Properties.Label = value; }
        public override List<PlotProperties> PlotProperties => new() { Properties };
        #endregion
        #region METHODS
        [JsonConstructor]
        public Signal(PlotProperties properties, ModuleType source, bool isPlotted, bool isAcquired) : base(source)
        {
            Properties = properties ?? new PlotProperties();
            IsPlotted = isPlotted;
            IsAcquired = isAcquired;
            InitializePlot();
        }
        public Signal(ModuleType source, PlotProperties? properties = null) : base(source)
        {
            Properties = properties ?? new PlotProperties();
            InitializePlot();
        }
        private void InitializePlot()
        {
            DataStreamer streamer;
            streamer = Plot.Add.DataStreamer(Properties.Capacity);
            streamer.LegendText = this.ToString();
            streamer.ManageAxisLimits = true;
            Plot.Axes.SetLimitsY(Properties.LowerBound, Properties.UpperBound);
            Plot.Axes.Bottom.TickLabelStyle.IsVisible = false;


            streamer.Axes.YAxis = Plot.Axes.Right;
            Plot.Grid.YAxis = Plot.Axes.Right;
            Plot.Axes.Left.RemoveTickGenerator();
        }
        public override void Update(Dictionary<int, List<object>> data)
        {
            var values = data.First().Value;

            if (values.FirstOrDefault() is List<object>)
            {
                foreach (var entry in values)
                {
                    if (entry is not List<object> nestedList)
                        continue;

                    int channelCount = nestedList.Count;

                    // Ensure we have enough streamers
                    while (Streamers.Count < channelCount)
                    {
                        var newStreamer = Plot.Add.DataStreamer(Properties.Capacity);
                        newStreamer.LegendText = $"{Properties.Label} Ch{Streamers.Count}";
                        newStreamer.ManageAxisLimits = true;  // crucial for live updates
                        newStreamer.LineWidth = 2;
                        newStreamer.LineColor = ScottPlot.Colors.Category10[Streamers.Count % 10];
                        Streamers.Add(newStreamer);
                    }

                    // Add one value per channel
                    for (int ch = 0; ch < channelCount; ch++)
                    {
                        double value = Convert.ToDouble(nestedList[ch]);
                        Streamers[ch]?.Add(value);
                    }
                }
            }
            else
            {
                // Single-channel case
                if (Streamers.Count == 0)
                {
                    var streamer = Plot.Add.DataStreamer(Properties.Capacity);
                    streamer.LegendText = Properties.Label;
                    streamer.ManageAxisLimits = true;
                    streamer.LineWidth = 2;
                    Streamers.Add(streamer);
                }

                foreach (var v in values)
                {
                    Streamers[0]?.Add(Convert.ToDouble(v));
                }
            }
        }


        public override string ToString() => Properties.Label??"~";
        #endregion
    }
}
