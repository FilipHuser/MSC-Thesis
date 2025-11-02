using ScottPlot;
using DataHub.Core;
using ScottPlot.Plottables;
using System.Text.Json.Serialization;

namespace Graphium.Models
{
    public class Signal
    {
        #region PROPERTIES
        public string Name { get; set; }
        public ModuleType Source { get; set; }
        [JsonIgnore] public bool IsPlotted { get; set; } = true;
        [JsonIgnore] public bool IsAcquired { get; set; } = true;
        [JsonIgnore] public Plot Plot { get; set; } = new Plot();
        [JsonIgnore] public List<DataLogger> Loggers { get; set; } = new();
        public PlotProperties Properties { get; set; } = new();
        #endregion
        #region METHODS
        public Signal()
        {
            Name = string.Empty;
            Source = ModuleType.NONE;
            Init();
        }
        public Signal(string name, ModuleType source, PlotProperties? properties = null)
        {
            Name = name;
            Source = source;
            Properties = properties ?? new PlotProperties();
            Init();
        }
        private void Init()
        {
            var newLogger = Plot.Add.DataLogger();
            newLogger.LineWidth = 2;
            newLogger.LineColor = GetColorForIndex(0);
            newLogger.Axes.YAxis = Plot.Axes.Right;
            newLogger.ManageAxisLimits = false;
            Loggers.Add(newLogger);
        }
        public void Update(DateTime timestamp, List<object> data)
        {
            if (data == null || data.Count == 0)
                return;

            double oaDate = timestamp.ToOADate();

            if (data.First() is IEnumerable<object> nested)
            {
                var nestedData = data.Cast<IEnumerable<object>>().ToList();
                int channelCount = nestedData.First().Count();
                while (Loggers.Count < channelCount)
                {
                    var newLogger = Plot.Add.DataLogger();
                    newLogger.LineWidth = 2;
                    newLogger.LineColor = GetColorForIndex(Loggers.Count);
                    newLogger.Axes.YAxis = Plot.Axes.Right;
                    newLogger.ManageAxisLimits = false;
                    Loggers.Add(newLogger);
                }

                foreach (var sample in nestedData)
                {
                    int ch = 0;
                    foreach (var value in sample)
                    {
                        Loggers[ch].Add(oaDate, Convert.ToDouble(value));
                        ch++;
                    }
                }
            }
            else
            {
                int sampleIndex = 0;
                foreach (var value in data)
                {
                    Loggers.First().Add(oaDate, Convert.ToDouble(value));
                    sampleIndex++;
                }
            }
        }
        private Color GetColorForIndex(int index)
        {
            var palette = new ScottPlot.Palettes.Nord();
            return palette.GetColor(index);
        }
        public override bool Equals(object? obj)
        {
            if (obj is not Signal other) return false;
            return Name == other.Name && Source == other.Source;
        }
        public override int GetHashCode() => HashCode.Combine(Name, Source);
        #endregion
    }
}