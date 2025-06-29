using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataHub.Core;

namespace Graphium.Models
{
    internal class SignalBase
    {
        #region PROPERTIES
        public Type? Source { get; set; }
        public Graph Graph { get; set; } = new Graph();
        #endregion
        #region METHODS
        public SignalBase(Type source)
        {
            Source = source;
        }
        public override string ToString() => $"{Graph.Label}";
        #endregion
    }
}
