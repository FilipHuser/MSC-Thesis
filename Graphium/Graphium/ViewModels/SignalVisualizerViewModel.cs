using Graphium.Interfaces;
using Graphium.Models;

namespace Graphium.ViewModels
{
    public abstract class SignalVisualizerViewModel<TSignal> : ViewModelBase, ISignalVisualizerViewModel where TSignal : SignalBase
    {
        #region PROPERTIES
        protected readonly TSignal Signal;
        #endregion
        #region METHODS
        protected SignalVisualizerViewModel(TSignal signal) => Signal = signal;
        public abstract void Clear();
        public abstract void Refresh();
        #endregion
    }
}