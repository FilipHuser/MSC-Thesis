using Graphium.Models;

namespace Graphium.Core
{
    internal class Sample
    {
        #region PROPERTIES
        private readonly DateTime _timestamp;
        public Dictionary<SignalBase, object> Channels = [];
        #endregion
        #region METHODS
        public Sample(DateTime timestamp)
        {
            _timestamp = timestamp;
        }
        public void AddValue(SignalBase signal, object value) => Channels.Add(signal, value);
        public DateTime GetTimestamp() => _timestamp;
        public object? this[SignalBase signal] { get => Channels.TryGetValue(signal, out var value) ? value : null; }
        #endregion
    }
}
