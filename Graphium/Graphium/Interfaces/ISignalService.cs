using System.Collections.ObjectModel;
using Graphium.Models;

namespace Graphium.Interfaces
{
    interface ISignalService
    {
        #region PROPERTIES
        public IReadOnlyCollection<SignalBase>? Signals { get; }
        event EventHandler? SignalsChanged;
        #endregion
        #region METHODS
        public void SetCurrentSignals(ObservableCollection<SignalBase> signals);
        public void AddSignal(SignalBase signal);
        public void RemoveSignal(SignalBase signal);
        public void Clear();
        #endregion
    }
}
