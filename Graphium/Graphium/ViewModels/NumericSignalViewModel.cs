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
        private ScottPlot.Color? _color;
        public bool NeedsXSync { get; set; } = false;
        public bool HasPlottables => _channelPlottables.Count > 0;
        public WpfPlot PlotControl { get; set; } = new();
        private double _yMin;
        private double _yMax;
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
        public void ScrollTo(double xMax, double viewWindowMs)
        {
            PlotControl.Plot.Axes.SetLimitsX(xMax - viewWindowMs, xMax);
        }
        public override void Refresh()
        {
            if (Signal.XData.Count == 0 || Signal.YData.Count == 0) return;
            bool wasEmpty = _channelPlottables.Count == 0;
            while (_channelPlottables.Count < Signal.YData.Count)
            {
                int ch = _channelPlottables.Count;
                var signalXY = PlotControl.Plot.Add.SignalXY(Signal.XData, Signal.YData[ch]);
                if (_color.HasValue)
                    signalXY.Color = _color.Value;
                _channelPlottables.Add(signalXY);
            }
            if (wasEmpty)
            {
                PlotControl.Plot.Axes.AutoScale();
                NeedsXSync = true;
            }
            PlotControl.Refresh();
        }
        public void SetColor(ScottPlot.Color color)
        {
            _color = color;
            _channelPlottables.ForEach(x => x.Color = color);
        }
        private void Init()
        {
            PlotControl.Plot.Layout.Fixed(new PixelPadding(0, 0, 0, 0));
            PlotControl.Plot.Axes.Left.IsVisible = false;
            PlotControl.Plot.Axes.Right.IsVisible = false;
            PlotControl.Plot.Axes.Top.IsVisible = false;
            PlotControl.Plot.RenderManager.RenderFinished += (_, _) =>
            {
                var limits = PlotControl.Plot.Axes.GetLimits();
                if (limits.Bottom != _yMin) YMin = limits.Bottom;
                if (limits.Top != _yMax) YMax = limits.Top;
            };
        }
        #endregion
    }
}