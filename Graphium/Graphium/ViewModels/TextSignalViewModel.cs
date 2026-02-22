using Graphium.Models;
using System.Collections.ObjectModel;

namespace Graphium.ViewModels
{
public class TextSignalViewModel : SignalVisualizerViewModel<TextSignal>
{
    private string _lastValue = string.Empty;
    private double _lastTime;

    public string LastValue { get => _lastValue; set => SetProperty(ref _lastValue, value); }
    public double LastTime { get => _lastTime; set => SetProperty(ref _lastTime, value); }

    public TextSignalViewModel(TextSignal signal) : base(signal) { }
    public override void Clear()
    {
        LastValue = string.Empty;
        LastTime = 0;
    }
    public override void Refresh()
    {
        if (Signal.YData.Count == 0) return;
        LastValue = Signal.YData[^1];
        LastTime = Signal.XData[^1];
    }
}
}