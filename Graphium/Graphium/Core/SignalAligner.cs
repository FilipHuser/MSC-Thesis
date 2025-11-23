using DataHub.Core;
using Graphium.Models;

internal class SignalAligner
{
    #region PROPERTIES
    private Dictionary<ModuleType, double> _sourceLastTimestamp = new();
    private Dictionary<Signal, object?> _signalLastValue = new();
    #endregion
    #region METHODS
    public void UpdateSignal(Signal signal, double timestamp, object value)
    {
        _sourceLastTimestamp[signal.Source] = timestamp;
        _signalLastValue[signal] = value;
    }
    public double GetLastTimestamp(ModuleType source) => _sourceLastTimestamp.TryGetValue(source, out var timestamp) ? timestamp : 0;
    public object? GetLastValue(Signal signal) => _signalLastValue.TryGetValue(signal, out var value) ? value : null;
    public double GetMaxTimestamp() => _sourceLastTimestamp.Count == 0 ? 0 : _sourceLastTimestamp.Values.Max();
    public void Clear()
    {
        _sourceLastTimestamp.Clear();
        _signalLastValue.Clear();
    }
    #endregion
}