using ScottPlot;
using ScottPlot.WPF;
using Graphium.Interfaces;
using System.Windows.Input;
using ScottPlot.MultiplotLayouts;

namespace Graphium.ViewModels
{
    internal class DataPlotterViewModel : ViewModelBase
    {
        #region SERVICES
        private readonly ISignalService _signalService;
        private readonly ILoggingService _loggingService;
        #endregion
        #region PROPERTIES
        private DraggableRows? _layout;
        private int? _dividerBeingDragged;
        private bool _mouseDragging = false;
        private double _timeWindowSec = 5;
        private bool _autoFollow = true;
        private readonly IMultiplot _multiplot;
        public IPalette PlotPallete;
        public WpfPlot PlotControl { get; } = new WpfPlot();
        #endregion
        #region METHODS
        public DataPlotterViewModel(ISignalService signalService, ILoggingService loggingService)
        {
            _signalService = signalService;
            _loggingService = loggingService;
            PlotPallete = new ScottPlot.Palettes.Nord();
            _multiplot = PlotControl.Multiplot;
            Init();
        }
        public void SetTimeWindow(double seconds)
        {
            _timeWindowSec = seconds;
        }
        public void SetAutoFollow(bool enabled)
        {
            _autoFollow = enabled;
        }
        public void OnSignalsChanged()
        {
            PlotControl.Reset();
            _multiplot.RemovePlot(_multiplot.GetPlot(0));
            UnsubscribePlotEvents();
            var signals = _signalService.Signals?.ToList();
            if (signals == null || !signals.Any()) { return; }

            int colorIndex = 0;
            foreach (var signal in signals)
            {
                signal.ResetTimestamp();
                var plot = signal.Plot;
                _multiplot.AddPlot(plot);
                var color = PlotPallete.GetColor(colorIndex++);
                colorIndex %= PlotPallete.Count();
                plot.Axes.Left.Label.Text = signal.Name;
                signal.Loggers.First().Color = color;
            }

            _multiplot.CollapseVertically();
            var plots = _multiplot.GetPlots();
            var bottomPlot = plots.Last();


            foreach (var plot in plots)
            {
                plot.Axes.Left.LockSize(32);
                plot.Axes.Right.LockSize(64);
                plot.Grid.XAxis = bottomPlot.Axes.Bottom;
                plot.Grid.YAxis = plot.Axes.Right;
            }

            _multiplot.SharedAxes.ShareX(plots);
            _layout = new DraggableRows();
            _multiplot.Layout = _layout;
            _dividerBeingDragged = null;
            SubscribePlotEvents();

            UpdateTimeWindow();

            PlotControl.Refresh();
            _loggingService.LogDebug($"Refreshed plot with {signals.Count()} signals.");
        }
        public void Update()
        {
            if (_autoFollow)
            {
                UpdateTimeWindow();
            }
            PlotControl.Refresh();
        }
        private void UpdateTimeWindow()
        {
            var signals = _signalService.Signals?.ToList();
            if (signals == null || !signals.Any()) return;

            var plots = _multiplot.GetPlots();
            if (!plots.Any()) return;

            // Find the latest timestamp across all signals (in seconds)
            double? maxTimestamp = signals
                .Where(s => s._lastTimestamp.HasValue)
                .Select(s => s._lastTimestamp.Value)
                .DefaultIfEmpty(0)
                .Max();

            if (!maxTimestamp.HasValue || maxTimestamp.Value == 0)
                return;

            double endTime = maxTimestamp.Value;
            double startTime = Math.Max(0, endTime - _timeWindowSec);

            foreach (var plot in plots)
            {
                plot.Axes.SetLimitsX(startTime, endTime);
                plot.Axes.AutoScaleY(plot.Axes.Right);
            }
        }
        private void Init() => _multiplot.RemovePlot(_multiplot.GetPlot(0));
        private void SubscribePlotEvents()
        {
            PlotControl.MouseDown += OnPlotMouseDown;
            PlotControl.MouseUp += OnPlotMouseUp;
            PlotControl.MouseMove += OnPlotMouseMove;
            PlotControl.MouseLeave += OnPlotMouseLeave;
            PlotControl.MouseDoubleClick += OnPlotMouseDoubleClick;
            PlotControl.KeyDown += OnPlotKeyDown;
        }
        private void UnsubscribePlotEvents()
        {
            PlotControl.MouseDown -= OnPlotMouseDown;
            PlotControl.MouseUp -= OnPlotMouseUp;
            PlotControl.MouseMove -= OnPlotMouseMove;
            PlotControl.MouseLeave -= OnPlotMouseLeave;
            PlotControl.MouseDoubleClick -= OnPlotMouseDoubleClick;
            PlotControl.KeyDown -= OnPlotKeyDown;
        }
        private void OnPlotMouseDown(object s, MouseButtonEventArgs e)
        {
            if (_layout == null) return;

            double mouseY = e.GetPosition(PlotControl).Y;
            _dividerBeingDragged = _layout.GetDivider((float)mouseY);

            if (_dividerBeingDragged is null)
            {
                _mouseDragging = true;
                _autoFollow = false;
            }

            PlotControl.UserInputProcessor.IsEnabled = _dividerBeingDragged is null;
        }
        private void OnPlotMouseUp(object s, MouseButtonEventArgs e)
        {
            if (_dividerBeingDragged is not null)
            {
                _dividerBeingDragged = null;
                PlotControl.UserInputProcessor.IsEnabled = true;
            }

            _mouseDragging = false;
        }
        private void OnPlotMouseMove(object s, MouseEventArgs e)
        {
            if (_layout == null) return;

            double mouseY = e.GetPosition(PlotControl).Y;

            if (_dividerBeingDragged is not null)
            {
                _layout.SetDivider(_dividerBeingDragged.Value, (float)mouseY);
                PlotControl.Refresh();
            }

            UpdateCursor(mouseY);
        }
        private void OnPlotMouseLeave(object s, MouseEventArgs e)
        {
            Mouse.OverrideCursor = null;
        }
        private void OnPlotMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Double-click to re-enable auto-follow
            _autoFollow = true;
            _loggingService.LogDebug("Auto-follow re-enabled");
        }
        private void OnPlotKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F)
            {
                // Press 'F' to toggle auto-follow
                _autoFollow = !_autoFollow;
                _loggingService.LogDebug($"Auto-follow {(_autoFollow ? "enabled" : "disabled")}");
            }
        }
        private void UpdateCursor(double mouseY)
        {
            Mouse.OverrideCursor = _layout?.GetDivider((float)mouseY) != null
                ? Cursors.SizeNS
                : Cursors.Arrow;
        }
        #endregion
    }
}