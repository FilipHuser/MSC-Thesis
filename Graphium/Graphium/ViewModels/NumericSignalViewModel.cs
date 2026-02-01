using System.Xml.Serialization;
using ScottPlot;
using ScottPlot.Plottables;
using ScottPlot.WPF;
using Signal = Graphium.Models.Signal;

namespace Graphium.ViewModels
{
    public class NumericSignalViewModel : SignalVisualizerViewModel
    {
        #region PROPERTIES
        private readonly List<SignalXY> _channelPlottables = new();
        public WpfPlot PlotControl { get; set; } = new();
        #endregion
        #region METHODS
        public NumericSignalViewModel(Signal signal) : base(signal)
        {
            Init();
        }
        public override void Clear()
        {
            foreach(var plottable in _channelPlottables)
            {
                PlotControl.Plot.Remove(plottable);
            }
            _channelPlottables.Clear();
        }
        public override void Refresh()
        {
            while (_channelPlottables.Count < Signal.YData.Count)
            {
                int ch = _channelPlottables.Count;
                var signalXY = PlotControl.Plot.Add.SignalXY(Signal.XData, Signal.YData[ch]);
                _channelPlottables.Add(signalXY);
            }
            PlotControl.Refresh();
        }
        public void SetColor(ScottPlot.Color color) => _channelPlottables.Select(x => x.Color = color);
        public void SetChannelColor(ScottPlot.Color color, int index)
        {

        }
        private void Init()
        {
            var plot = PlotControl.Plot.Add.SignalXY(Signal.XData, Signal.YData.First());
            _channelPlottables.Add(plot);
        }
        #endregion
    }
}
