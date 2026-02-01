using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Graphium.Models;

namespace Graphium.ViewModels
{
    public class TextSignalViewModel : SignalVisualizerViewModel
    {
        #region PROPERTIES
        #endregion
        #region METHODS
        public TextSignalViewModel(Signal signal) : base(signal)
        {
            
        }
        public override void Clear()
        {

        }

        public override void Refresh()
        {
        }
        #endregion
    }
}
