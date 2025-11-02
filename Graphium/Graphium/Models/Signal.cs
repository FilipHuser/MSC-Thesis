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
        public double SamplingRate { get; set; }
        [JsonIgnore] public double? _lastTimestamp = null;
        [JsonIgnore] private double _timeOffset = 0; // ADD THIS LINE
        #endregion
        #region METHODS
        public Signal()
        {
            Name = string.Empty;
            Source = ModuleType.NONE;
            Init();
        }
        public Signal(string name, ModuleType source, PlotProperties? properties = null, double samplingRate = 1000)
        {
            Name = name;
            Source = source;
            Properties = properties ?? new PlotProperties();
            SamplingRate = samplingRate;
            Init();
        }

        public void ResetTimestamp()
        {
            _lastTimestamp = null;
            _timeOffset = 0;
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
            double currentTime;
            if (!_lastTimestamp.HasValue)
            {
                _timeOffset = timestamp.Ticks / (double)TimeSpan.TicksPerSecond;
                currentTime = 0;
            }
            else
            {
                currentTime = (timestamp.Ticks / (double)TimeSpan.TicksPerSecond) - _timeOffset;
            }

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
                        Loggers[ch].Add(currentTime, Convert.ToDouble(value));
                        ch++;
                    }
                }
            }
            else
            {
                foreach (var value in data)
                {
                    Loggers.First().Add(currentTime, Convert.ToDouble(value));
                }
            }

            _lastTimestamp = currentTime;
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