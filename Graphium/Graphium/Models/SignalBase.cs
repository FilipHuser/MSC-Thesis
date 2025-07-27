using System.Xml.Linq;
using Graphium.Models;

namespace Graphium.Models
{
    public abstract class SignalBase
    {
        #region PROPERTIES

        public Type? Source;
        public abstract int Count { get; }
        public bool IsPlotted { get; set; }
        public bool IsAcquired { get; set; }
        #endregion
        #region METHODS
        public SignalBase(Type source)
        {
            Source = source;
        }
        public abstract void Update(Dictionary<int , List<object>> data);
        #endregion
    }
}
