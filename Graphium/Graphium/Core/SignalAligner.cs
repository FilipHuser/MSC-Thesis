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
        // Store the timestamp for the source
        _sourceLastTimestamp[signal.Source] = timestamp;

        // Store the value for this specific signal
        _signalLastValue[signal] = value;
    }

    public double GetLastTimestamp(ModuleType source)
    {
        if (_sourceLastTimestamp.TryGetValue(source, out var timestamp))
        {
            return timestamp;
        }
        return 0;
    }

    public object? GetLastValue(Signal signal)
    {
        if (_signalLastValue.TryGetValue(signal, out var value))
        {
            return value;
        }
        return null;
    }

    public double GetMaxTimestamp()
    {
        if (_sourceLastTimestamp.Count == 0) return 0;
        return _sourceLastTimestamp.Values.Max();
    }

    public void Clear()
    {
        _sourceLastTimestamp.Clear();
        _signalLastValue.Clear();
    }
    #endregion
}