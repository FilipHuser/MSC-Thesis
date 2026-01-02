using Graphium.Models;

namespace Graphium.Core
{
    internal class MeasurementDataRow
    {
        #region PROPERTIES
        public double Timestamp { get; set; }
        public Dictionary<Signal, object> Values { get; set; } = new();
        #endregion
    }
}
