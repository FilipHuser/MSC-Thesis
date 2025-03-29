using System.Collections.ObjectModel;
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
        public int PointLimit { get; set; } = 5000;
        [XmlIgnore]
        public WpfPlot PlotControl { get; } = new WpfPlot();
        [XmlIgnore]
        public DataStreamer Streamer { get; set; }

        public Graph()
        {
            Streamer = PlotControl.Plot.Add.DataStreamer(PointLimit);
            PlotControl.UserInputProcessor.Disable();
        }
        public object Clone() => this.MemberwiseClone();
    }
}
