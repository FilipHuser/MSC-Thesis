using DataHub.Core;
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
        [JsonIgnore] public List<double> XData { get; set; } = new(); // Single shared X data
        [JsonIgnore] public List<List<double>> YData { get; set; } = new();
        public PlotProperties Properties { get; set; } = new();
        public double SamplingRate { get; set; }
        #endregion

        #region METHODS
        public Signal()
        {
            Name = string.Empty;
            Source = ModuleType.NONE;
            AddChannel();
        }

        public Signal(string name, ModuleType source, double samplingRate, PlotProperties? properties = null)
        {
            Name = name;
            Source = source;
            Properties = properties ?? new PlotProperties();
            SamplingRate = samplingRate;
            AddChannel();
        }

        private void AddChannel()
        {
            YData.Add(new List<double>());
        }

        public void Update(double absoluteTimeSeconds, object? data)
        {
            if (data == null) return;

            if (data is IEnumerable<object> nestedData)
            {
                int channelCount = nestedData.Count();

                while (YData.Count < channelCount) { AddChannel(); }

                int ch = 0;

                foreach (var sample in nestedData)
                {

                    YData[ch].Add(Convert.ToDouble(sample));
                    ch++;
                }
            } else {
                YData[0].Add(Convert.ToDouble(data));
            }
            XData.Add(absoluteTimeSeconds);
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