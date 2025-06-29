using System.Xml.Serialization;

namespace Graphium.Models
{
    [Serializable]
    public class Graph
    {
        #region PROPERTIES
        public string? Label { get; set; }
        public double LowerBound { get; set; } = -10;
        public double UpperBound { get; set; } = 10;
        public int PointLimit { get; set; } = 1000;
        #endregion  
        #region METHODS
        public object Clone() => this.MemberwiseClone();
        #endregion
    }
}
