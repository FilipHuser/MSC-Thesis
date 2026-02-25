using DataHub.Core;
using Graphium.Models;

internal class SignalAligner
{
    #region PROPERTIES
    private Dictionary<SignalBase, object?> _signalLastValue = new();
    #endregion
    #region METHODS
    public void UpdateSignal(SignalBase signal, object value)
    {
        _signalLastValue[signal] = value;
    }
    public object? GetLastValue(SignalBase signal) => _signalLastValue.TryGetValue(signal, out var value) ? value : null;
    public void Clear() => _signalLastValue.Clear();
    #endregion
}