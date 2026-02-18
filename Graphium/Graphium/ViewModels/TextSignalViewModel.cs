using Graphium.Models;

namespace Graphium.ViewModels
{
    public class TextSignalViewModel : SignalVisualizerViewModel<TextSignal>
    {
        #region PROPERTIES
        #endregion
        #region METHODS
        public TextSignalViewModel(TextSignal signal) : base(signal)
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