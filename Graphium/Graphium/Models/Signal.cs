using System.Data;
using System.IO;
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
        public override int Count => 1;
        public PlotProperties Properties { get; set; }
        [JsonIgnore]
        public WpfPlot PlotControl { get; set; } = new WpfPlot();
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
            Init();
        }
        public Signal(ModuleType source, PlotProperties? properties = null) : base(source)
        {
            Properties = properties ?? new PlotProperties();
            Init();
        }
        private void Init()
        {
            var plot = PlotControl.Plot;
            PlotControl.UserInputProcessor.IsEnabled = false;
            DataStreamer streamer;
            streamer = plot.Add.DataStreamer(Properties.Capacity);
            streamer.ManageAxisLimits = true;
            plot.Axes.SetLimitsY(Properties.LowerBound, Properties.UpperBound);
        }
        public override void Update(Dictionary<int, List<object>> data)
        {
            var plot = PlotControl.Plot;
            var values = data.First().Value;

            if (values.FirstOrDefault() is List<object>)
            {
                foreach (var entry in values)
                {
                    if (entry is not List<object> nestedList)
                        continue;

                    int channelCount = nestedList.Count;
                    while (Streamers.Count < channelCount)
                    {
                        var newStreamer = plot.Add.DataStreamer(Properties.Capacity);
                        newStreamer.LegendText = $"{Properties.Label} Ch{Streamers.Count}";
                        newStreamer.ManageAxisLimits = true;
                        newStreamer.LineWidth = 2;
                        newStreamer.LineColor = ScottPlot.Colors.Category10[Streamers.Count % 10];
                        Streamers.Add(newStreamer);
                    }
                    for (int ch = 0; ch < channelCount; ch++)
                    {
                        double value = Convert.ToDouble(nestedList[ch]);
                        Streamers[ch]?.Add(value);
                    }
                }
            } else {
                if (Streamers.Count == 0)
                {
                    var streamer = plot.Add.DataStreamer(Properties.Capacity);
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
