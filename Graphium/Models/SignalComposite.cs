using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graphium.Models
{
    internal class SignalComposite : SignalBase
    {
        #region PROPERTIES
        protected List<SignalBase> _signals = new List<SignalBase>();
        public List<Graph> Graphs => _signals.Select(x => x.Graph).ToList();
        #endregion
        #region METHODS
        public SignalComposite(Type type) : base(type){}

        public override string ToString() => string.Join(",", Graphs.Select(x => x.Label));
        #endregion
    }
}
