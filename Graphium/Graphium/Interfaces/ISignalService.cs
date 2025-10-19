using System.Collections.ObjectModel;
using Graphium.Models;

namespace Graphium.Interfaces
{
    public class SignalChangedEventArgs : EventArgs
    {
        public SignalBase? Added { get; set; }
        public SignalBase? Removed { get; set; }
    }

    interface ISignalService
    {
        #region PROPERTIES
        public IReadOnlyCollection<SignalBase>? Signals { get; }
        event EventHandler<SignalChangedEventArgs> SignalsChanged;
        #endregion
        #region METHODS
        public void SetCurrentSignals(ObservableCollection<SignalBase> signals);
        public void AddSignal(SignalBase signal);
        public void RemoveSignal(SignalBase signal);
        public void Clear();
        #endregion
    }
}
