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
        public List<DataLogger?> DataLoggers { get; set; } = [];
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
            //PlotControl.UserInputProcessor.IsEnabled = false;
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
                    while (DataLoggers.Count < channelCount)
                    {
                        var logger = plot.Add.DataLogger();
                        logger.LegendText = $"CH{DataLoggers.Count}";
                        logger.ManageAxisLimits = true;
                        logger.LineWidth = 2;
                        logger.LineColor = ScottPlot.Colors.Category10[DataLoggers.Count % 10];
                        DataLoggers.Add(logger);
                    }
                    for (int ch = 0; ch < channelCount; ch++)
                    {
                        double value = Convert.ToDouble(nestedList[ch]);
                        DataLoggers[ch]?.Add(value);
                    }
                }
            } else {
                if (DataLoggers.Count == 0)
                {
                    var logger = plot.Add.DataLogger();
                    logger.ManageAxisLimits = true;
                    logger.LineWidth = 2;
                    DataLoggers.Add(logger);
                }

                foreach (var v in values)
                {
                    DataLoggers[0]?.Add(Convert.ToDouble(v));
                }
            }
        }
        public override string ToString() => Properties.Label??"~";
        #endregion
    }
}
