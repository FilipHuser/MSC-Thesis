using System.Xml.Serialization;
using ScottPlot.Plottables;

namespace Graphium.Models
{
    [Serializable]
    public class PlotProperties
    {
        #region PROPERTIES
        public string? Unit { get; set; } = "~";
        public double LowerBound { get; set; } = -10;
        public double UpperBound { get; set; } = 10;
        #endregion

        #region METHODS
        public PlotProperties() { }
        public PlotProperties(PlotProperties other)
        {
            Unit = other.Unit;
            LowerBound = other.LowerBound;
            UpperBound = other.UpperBound;
        }

        public object Clone() => this.MemberwiseClone();
        #endregion
    }
}