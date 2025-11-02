using System.Collections.ObjectModel;
using Graphium.Models;

namespace Graphium.Interfaces
{
    interface ISignalService
    {
        #region PROPERTIES
        public IReadOnlyCollection<Signal>? Signals { get; }
        event EventHandler? SignalsChanged;
        #endregion
        #region METHODS
        public void SetCurrentSignals(ObservableCollection<Signal> signals);
        public void AddSignal(Signal signal);
        public void RemoveSignal(Signal signal);
        public void Clear();
        #endregion
    }
}
