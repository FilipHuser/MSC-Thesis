using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Graphium.Interfaces;
using Graphium.Models;

namespace Graphium.Services
{
    class SignalService : ISignalService
    {
        #region PROPERTIES
        private ObservableCollection<SignalBase>? _signals = [];
        public IReadOnlyCollection<SignalBase>? Signals => _signals;
        public event EventHandler? SignalsChanged;
        #endregion
        #region METHODS
        public void AddSignal(SignalBase signal)
        {
            if (_signals == null) { _signals = new ObservableCollection<SignalBase>(); }
            _signals.Add(signal);
        }
        public void RemoveSignal(SignalBase signal) => _signals?.Remove(signal);
        public void Clear() => _signals?.Clear();
        public void SetCurrentSignals(ObservableCollection<SignalBase> signals)
        {
            if(_signals != null) {
                _signals.CollectionChanged -= SignalsCollectionChanged; }
            _signals = signals;
            if(_signals != null) { 
                _signals.CollectionChanged += SignalsCollectionChanged; }
            SignalsChanged?.Invoke(this, EventArgs.Empty);
        }
        private void SignalsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => SignalsChanged?.Invoke(this, EventArgs.Empty);
        #endregion
    }
}
