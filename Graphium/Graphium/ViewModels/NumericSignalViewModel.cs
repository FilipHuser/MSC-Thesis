using ScottPlot;
using ScottPlot.Plottables;
using ScottPlot.WPF;
using Graphium.Models;

namespace Graphium.ViewModels
{
    public class NumericSignalViewModel : SignalVisualizerViewModel<NumericSignal>
    {
        #region PROPERTIES
        private readonly List<SignalXY> _channelPlottables = new();
        public WpfPlot PlotControl { get; set; } = new();
        #endregion
        #region METHODS
        public NumericSignalViewModel(NumericSignal signal) : base(signal)
        {
            Init();
        }
        public override void Clear()
        {
            foreach (var plottable in _channelPlottables)
                PlotControl.Plot.Remove(plottable);
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
        public void HideXAxis()
        {
            var bottom = PlotControl.Plot.Axes.Bottom;
            bottom.TickLabelStyle.IsVisible = false;
            bottom.Label.IsVisible = false;
            bottom.MajorTickStyle.Length = 0;
            bottom.MinorTickStyle.Length = 0;
            PlotControl.Plot.Layout.Fixed(new PixelPadding(60, 10, 0, 0));
        }
        public void SetColor(ScottPlot.Color color) => _channelPlottables.ForEach(x => x.Color = color);
        private void Init()
        {
            var plot = PlotControl.Plot.Add.SignalXY(Signal.XData, Signal.YData.First());
            PlotControl.Plot.Layout.Fixed(new PixelPadding(60, 10, 0, 10));
            _channelPlottables.Add(plot);
        }
        #endregion
    }
}