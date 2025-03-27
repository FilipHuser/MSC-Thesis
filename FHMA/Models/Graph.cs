using System.Collections.ObjectModel;
using System.Xml.Serialization;
using ScottPlot;
using ScottPlot.Plottables;
using ScottPlot.WPF;

namespace FHMA.Models
{
    public enum ModuleType
    {
        EKG,    // Electrocardiogram
        EEG,    // Electroencephalogram
        HR,     // Heart Rate
        RESP,   // Respiration
        SPO2,   // Oxygen Saturation
        EDA,    // Electrodermal Activity
        AN,     // Analog
    }

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
        public ModuleType ModuleType { get; set; }
        public double LowerBound { get; set; }
        public double UpperBound { get; set; }
        public int PointLimit { get; set; } = 10000;
        [XmlIgnore]
        public  WpfPlot PlotControl { get; } = new WpfPlot();
        [XmlIgnore]
        public DataStreamer Streamer { get; set; }

        public Graph()
        {
            PlotControl.UserInputProcessor.Disable();
            Streamer = PlotControl.Plot.Add.DataStreamer(PointLimit);
        }
    }
}
