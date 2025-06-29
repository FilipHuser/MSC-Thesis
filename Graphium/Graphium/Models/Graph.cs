using System.Xml.Serialization;
using ScottPlot.Plottables;

namespace Graphium.Models
{
    [Serializable]
    public class Graph
    {
        #region PROPERTIES
        public string? Label { get; set; }
        public double LowerBound { get; set; } = -10;
        public double UpperBound { get; set; } = 10;
        public int Capacity { get; set; } = 1000;
        #endregion  
        #region METHODS
        public object Clone() => this.MemberwiseClone();
        #endregion
    }
}
