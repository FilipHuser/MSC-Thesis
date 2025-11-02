using System.Windows.Input;
using System.Windows.Threading;
using Graphium.Interfaces;
using NLog;
using ScottPlot;
using ScottPlot.MultiplotLayouts;
using ScottPlot.Palettes;
using ScottPlot.Plottables;
using ScottPlot.WPF;

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
        private double _timeWindowSec = 10;
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
                var plot = signal.Plot;
                _multiplot.AddPlot(plot);
                var color = PlotPallete.GetColor(colorIndex++);
                colorIndex %= PlotPallete.Count();
                plot.Axes.Left.Label.Text = signal.Name;
                signal.Loggers.First().Color = color;
                plot.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.DateTimeAutomatic();
            }

            _multiplot.CollapseVertically();
            var plots = _multiplot.GetPlots();
            var bottomPlot = plots.Last();

            var tickGen = (ScottPlot.TickGenerators.DateTimeAutomatic)bottomPlot.Axes.Bottom.TickGenerator;
            tickGen.LabelFormatter = dt =>
            {

                var range = bottomPlot.Axes.Bottom.Max - bottomPlot.Axes.Bottom.Min;

                if (range < 1.0 / 24 / 60)
                    return $"{dt:ss.fff}";
                else if (range < 1.0 / 24)
                    return $"{dt:mm:ss}";
                else
                    return $"{dt:ss}";
            };

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
            PlotControl.Refresh();
            _loggingService.LogDebug($"Refreshed plot with {signals.Count()} signals.");
        }
        public void Update() => PlotControl.Refresh();
        private void Init() => _multiplot.RemovePlot(_multiplot.GetPlot(0));
        private void SubscribePlotEvents()
        {
            PlotControl.MouseDown += OnPlotMouseDown;
            PlotControl.MouseUp += OnPlotMouseUp;
            PlotControl.MouseMove += OnPlotMouseMove;
            PlotControl.MouseLeave += OnPlotMouseLeave;
        }
        private void UnsubscribePlotEvents()
        {
            PlotControl.MouseDown -= OnPlotMouseDown;
            PlotControl.MouseUp -= OnPlotMouseUp;
            PlotControl.MouseMove -= OnPlotMouseMove;
            PlotControl.MouseLeave -= OnPlotMouseLeave;
        }
        private void OnPlotMouseDown(object s, MouseButtonEventArgs e)
        {
            if (_layout == null) return;

            double mouseY = e.GetPosition(PlotControl).Y;
            _dividerBeingDragged = _layout.GetDivider((float)mouseY);

            if (_dividerBeingDragged is null)
            {
                _mouseDragging = true;
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
        private void UpdateCursor(double mouseY)
        {
            Mouse.OverrideCursor = _layout?.GetDivider((float)mouseY) != null
                ? Cursors.SizeNS
                : Cursors.Arrow;
        }
        #endregion
    }
}