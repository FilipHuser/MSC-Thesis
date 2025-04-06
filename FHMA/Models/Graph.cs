using System.Runtime.Serialization;
using System.Xml.Serialization;
using ScottPlot;
using ScottPlot.Colormaps;
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
        #endregion  
        #region METHODS
        public object Clone() => this.MemberwiseClone();
        #endregion
    }
}
