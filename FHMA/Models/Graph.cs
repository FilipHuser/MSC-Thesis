using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using ScottPlot;
using ScottPlot.Plottables;
using ScottPlot.WPF;

namespace FHMA.Models
{
    [Serializable]
    public class Graph
    {
        private int _channel;
        public int Channel
        {
            get => _channel;
            set
            {
                if (value < 0) { throw new ArgumentOutOfRangeException(nameof(value), "Channel number cannot be negative!"); }
                _channel = value;
            }
        }
        public string? Label { get; set; }
        public double LowerBound { get; set; } = -10;
        public double UpperBound { get; set; } = 10;
        private int _pointLimit = 1000;
        public int PointLimit
        {
            get => _pointLimit;
            set
            {
                _pointLimit = value;
                Streamer = PlotControl.Plot.Add.DataStreamer(_pointLimit);
            }
        }
        [XmlIgnore]
        public WpfPlot PlotControl { get; } = new WpfPlot();
        [XmlIgnore]
        public DataStreamer? Streamer { get; set; }

        public Graph()
        {
            PlotControl.UserInputProcessor.Disable();
            PlotControl.Plot.Axes.AutoScale();
        }
        public object Clone() => this.MemberwiseClone();
    }
}
