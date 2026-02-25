using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using DataHub.Core;
using Graphium.Enums;

namespace Graphium.Models
{
    public class NumericSignal : SignalBase
    {
        #region PROPERTIES
        public double SamplingRate { get; set; }
        public PlotProperties Properties { get; set; } = new();
        [JsonIgnore] public List<List<double>> YData { get; set; } = new();
        #endregion
        #region METHODS
        public NumericSignal() { YData.Add(new List<double>()); }

        public NumericSignal(string name, ModuleType source, double samplingRate, PlotProperties? properties = null)
            : base(name, SignalType.Numeric, source)
        {
            SamplingRate = samplingRate;
            Properties = properties ?? new PlotProperties();
            YData.Add(new List<double>());
        }
        public override void Update(double absoluteTimeSeconds, object? data)
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
            }
            else
            {
                YData[0].Add(Convert.ToDouble(data));
            }
            XData.Add(absoluteTimeSeconds);
        }
        public override void ClearData()
        {
            base.ClearData();
            YData.ForEach(channel => channel.Clear());
        }
        private void AddChannel() => YData.Add(new List<double>());
        #endregion
    }
}
