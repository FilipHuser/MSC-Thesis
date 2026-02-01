using Graphium.Models;

namespace Graphium.ViewModels
{
    public abstract class SignalVisualizerViewModel : ViewModelBase
    {
        #region PROPERTIES
        protected readonly Signal Signal;
        #endregion
        #region METHODS
        protected SignalVisualizerViewModel(Signal signal) => Signal = signal;
        public abstract void Clear();
        public abstract void Refresh();
        #endregion
    }
}
