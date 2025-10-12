using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graphium.Models
{
    internal class ChannelSlot : ModelBase
    {
        #region PROPERTIES
        private int _number;
        private SignalBase? _signal;
        public int Number { get => _number; set => SetProperty(ref _number, value); }
        public SignalBase? Signal { get => _signal; set => SetProperty(ref _signal, value); }
        #endregion
        #region METHODS
        #endregion
    }
}