using System.Runtime.Serialization;
using System.Xml.Serialization;
using ScottPlot.Plottables;
using ScottPlot.WPF;

namespace FHMA.Models
{
    [Serializable]
    public class Graph
    {
        #region PROPERTIES
        public string? Label { get; set; }
        public double LowerBound { get; set; } = -10;
        public double UpperBound { get; set; } = 10;
        public int PointLimit { get; set; } = 1000;
        [XmlIgnore]
        public int? Quality { get; set; }
        [XmlIgnore]
        public WpfPlot PlotControl { get; } = new WpfPlot();
        [XmlIgnore]
        public DataStreamer? Streamer { get; set; }
        #endregion  
        public Graph() => InitPlot();
        #region METHODS
        public void InitPlot()
        {
            PlotControl.MinHeight = 150;
            PlotControl.MinWidth = 500;
            PlotControl.Plot.Title(Label);
            PlotControl.UserInputProcessor.Disable();
            Streamer = PlotControl.Plot.Add.DataStreamer(PointLimit);
            PlotControl.Plot.Axes.SetLimits(null , null , LowerBound , UpperBound);
        }
        public object Clone() => this.MemberwiseClone();
        #endregion
    }
}
