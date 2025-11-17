using ScottPlot;
using ScottPlot.WPF;
using Graphium.Interfaces;
using Graphium.Services;
using Graphium.Models;
using System.Windows.Input;
using ScottPlot.MultiplotLayouts;
using System.Windows;

namespace Graphium.ViewModels
{
    internal class DataPlotterViewModel : ViewModelBase
    {
        #region SERVICES
        private readonly ISignalService _signalService;
        private readonly ILoggingService _loggingService;
        private readonly SignalPlotManager _plotManager;
        #endregion
        #region PROPERTIES
        private DraggableRows? _layout;
        private int? _dividerBeingDragged;
        private bool _mouseDragging = false;
        private double _timeWindowSec = 5;
        private bool _autoFollow = true;
        private readonly IMultiplot _multiplot;
        public IPalette PlotPallete;
        public WpfPlot PlotControl { get; } = new();
        #endregion
        #region METHODS
        public DataPlotterViewModel(ISignalService signalService, ILoggingService loggingService)
        {
            _signalService = signalService;
            _loggingService = loggingService;
            PlotPallete = new ScottPlot.Palettes.Nord();
            _plotManager = new SignalPlotManager(PlotPallete);
            _multiplot = PlotControl.Multiplot;
            Init();
        }
        public void SetTimeWindow(double seconds) => _timeWindowSec = seconds;
        public void SetAutoFollow(bool enabled) => _autoFollow = enabled;
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
                var plot = _plotManager.GetOrCreatePlot(signal);
                _multiplot.AddPlot(plot);

                var color = PlotPallete.GetColor(colorIndex++);
                colorIndex %= PlotPallete.Count();

                // Set color for all channels in this signal
                _plotManager.SetAllChannelsColor(signal, color);
            }

            _multiplot.CollapseVertically();

            var plots = _multiplot.GetPlots();
            var bottomPlot = plots.Last();

            foreach (var plot in plots)
            {
                plot.Axes.Left.LockSize(32);
                plot.Axes.Right.LockSize(64);
                plot.Grid.YAxis = plot.Axes.Right;

                if (plot != bottomPlot)
                {
                    plot.Grid.XAxis = bottomPlot.Axes.Bottom;
                }
            }

            _multiplot.SharedAxes.ShareX(plots);

            _layout = new DraggableRows();
            _multiplot.Layout = _layout;
            _dividerBeingDragged = null;

            SubscribePlotEvents();
            PlotControl.UpdateLayout();
            PlotControl.Refresh();

            _loggingService.LogDebug($"Refreshed plot with {signals.Count()} signals.");
        }
        public void Update(double currentTime)
        {
            var signals = _signalService.Signals?.ToList();
            if (signals != null)
            {
                foreach (var signal in signals)
                {
                    _plotManager.UpdatePlot(signal);
                }
            }

            //if (_autoFollow)
            //{
            //    var plots = _multiplot.GetPlots();
            //    if (!plots.Any()) return;

            //    double startTime = Math.Max(0, currentTime - _timeWindowSec);

            //    foreach (var plot in plots)
            //    {
            //        plot.Axes.SetLimitsX(startTime, currentTime);
            //        plot.Axes.AutoScaleY(plot.Axes.Righ\t);
            //    }
            //}
            PlotControl.Refresh();
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
            double dpiScale = GetDPIScale();
            _dividerBeingDragged = _layout.GetDivider((float)(mouseY * dpiScale));

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
            double dpiScale = GetDPIScale();

            if (_dividerBeingDragged is not null)
            {
                _layout.SetDivider(_dividerBeingDragged.Value, (float)(mouseY * dpiScale));
                PlotControl.Refresh();
            }

            UpdateCursor(mouseY * dpiScale);
        }
        private void OnPlotMouseLeave(object s, MouseEventArgs e)
        {
            Mouse.OverrideCursor = null;
        }
        private void OnPlotMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            _autoFollow = true;
            _loggingService.LogDebug("Auto-follow re-enabled");
        }
        private void OnPlotKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F)
            {
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
        private double GetDPIScale()
        {
            var source = PresentationSource.FromVisual(PlotControl);
            if (source?.CompositionTarget != null)
            {
                return source.CompositionTarget.TransformToDevice.M22;
            }
            return 1.0;
        }
        #endregion
    }
}