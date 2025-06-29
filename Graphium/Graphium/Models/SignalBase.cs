using System.Xml.Linq;
using Graphium.Models;

namespace Graphium.Models
{
    public abstract class SignalBase
    {
        #region PROPERTIES
        public abstract int Count { get; }
        public Type? Source;
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
