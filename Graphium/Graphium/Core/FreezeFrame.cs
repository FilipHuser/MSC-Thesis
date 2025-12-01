using System.Collections.Concurrent;
using DataHub.Core;
using Graphium.Models;

public class FreezeFrame
{
    // Ukládá timestamp a data (může být single value nebo list)
    private readonly ConcurrentDictionary<Signal, (double timestamp, object value)> _currentValues = new();

    public void UpdateValue(Signal signal, double timestamp, object value)
    {
        _currentValues[signal] = (timestamp, value);
    }

    public Dictionary<Signal, (double timestamp, object value)> GetSnapshot()
    {
        return new Dictionary<Signal, (double timestamp, object value)>(_currentValues);
    }

    public double GetMaxTimestamp()
    {
        return _currentValues.Values.Select(v => v.timestamp).DefaultIfEmpty(0).Max();
    }
}