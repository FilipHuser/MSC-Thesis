using Graphium.Enums;
using Graphium.Models;

namespace Graphium.Core
{
    internal class SignalStorage
    {
        #region PROPERTIES
        private HoldMode _mode;
        private string? _filePath;
        private Dictionary<Signal, List<object>?> _latestSamples = [];
        private HashSet<Signal> _signalsWithData = [];
        public SignalStorage(string measurementName, List<Signal> signals, HoldMode mode = HoldMode.ZOH)
        {
            _mode = mode;
            foreach (Signal signal in signals)
            {
                _latestSamples.Add(signal, null);
            }
        }
        #endregion
        #region METHODS
        public void Add(Signal signal, List<object>? value)
        {
            _latestSamples[signal] = value;
            if (!_signalsWithData.Contains(signal)) { _signalsWithData.Add(signal); }
        }
        public Dictionary<Signal, List<object>?> GetSnapshot()
        {
            var snapshot = new Dictionary<Signal, List<object>?>();

            foreach(var kvp in _latestSamples)
            {
                var signal = kvp.Key;
                var value = kvp.Value;

                switch (_mode)
                {
                    case HoldMode.ZOH:
                        snapshot[signal] = value;
                        break;

                    case HoldMode.FOH:
                    case HoldMode.HOH:
                        // For interpolation modes, return null if no data yet
                        // (would need history for proper interpolation)
                        snapshot[signal] = _signalsWithData.Contains(signal) ? value : null;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException($"Unknown hold mode: {_mode}");
                }
            }

            return snapshot;
        }
        #endregion
    }
}