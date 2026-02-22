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
        private double _yMin;
        private double _yMax;
        private bool _autoScroll = false;
        private const double ViewWindowMs = 5000;

        public double YMin { get => _yMin; set => SetProperty(ref _yMin, value); }
        public double YMax { get => _yMax; set => SetProperty(ref _yMax, value); }
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
            if (Signal.XData.Count == 0 || Signal.YData.Count == 0) return;

            while (_channelPlottables.Count < Signal.YData.Count)
            {
                int ch = _channelPlottables.Count;
                var signalXY = PlotControl.Plot.Add.SignalXY(Signal.XData, Signal.YData[ch]);
                _channelPlottables.Add(signalXY);
            }

            if (_autoScroll)
            {
                double xMax = Signal.XData[^1];
                double xMin = xMax - ViewWindowMs;
                PlotControl.Plot.Axes.SetLimitsX(xMin, xMax);
            }

            PlotControl.Refresh();
        }

        public void SetAutoScroll(bool enabled) => _autoScroll = enabled;

        public void SetColor(ScottPlot.Color color) => _channelPlottables.ForEach(x => x.Color = color);

        private void Init()
        {
            PlotControl.Plot.Layout.Fixed(new PixelPadding(0, 0, 0, 0));
            PlotControl.Plot.Axes.Left.IsVisible = false;
            PlotControl.Plot.Axes.Right.IsVisible = false;
            PlotControl.Plot.Axes.Top.IsVisible = false;

            PlotControl.Plot.RenderManager.RenderFinished += (_, _) =>
            {
                var limits = PlotControl.Plot.Axes.GetLimits();
                YMin = limits.Bottom;
                YMax = limits.Top;
            };
        }
        #endregion
    }
}