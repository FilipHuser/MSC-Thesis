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

        public event EventHandler<SignalChangedEventArgs>? SignalsChanged;
        #endregion
        #region METHODS
        public void SetCurrentSignals(ObservableCollection<SignalBase>? signals)
        {
            if (_signals != null) { _signals.CollectionChanged -= Signals_CollectionChanged; }

            _signals = signals;

            if (_signals != null) { _signals.CollectionChanged += Signals_CollectionChanged; }

            SignalsChanged?.Invoke(this, new SignalChangedEventArgs());
        }

        private void Signals_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (SignalBase s in e.NewItems)
                    SignalsChanged?.Invoke(this, new SignalChangedEventArgs { Added = s });
            }

            if (e.OldItems != null)
            {
                foreach (SignalBase s in e.OldItems)
                    SignalsChanged?.Invoke(this, new SignalChangedEventArgs { Removed = s });
            }
        }

        public void AddSignal(SignalBase signal)
        {
            if (_signals == null) { _signals = new ObservableCollection<SignalBase>(); }
            _signals.Add(signal);
        }

        public void RemoveSignal(SignalBase signal) => _signals?.Remove(signal);

        public void Clear() => _signals?.Clear();
        #endregion
    }
}
