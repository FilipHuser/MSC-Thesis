using Graphium.Models;

namespace Graphium.Core
{
    internal class Sample
    {
        #region PROPERTIES
        private readonly DateTime _timestamp;
        public Dictionary<Signal,object> Channels = [];
        #endregion
        #region METHODS
        public Sample(DateTime timestamp)
        {
            _timestamp = timestamp;
        }
        public void AddValue(Signal signal, object value) => Channels.Add(signal, value);
        public DateTime GetTimestamp() => _timestamp;
        public object? this[Signal signal] { get => Channels.TryGetValue(signal, out var value) ? value : null; }
        #endregion
    }
}
