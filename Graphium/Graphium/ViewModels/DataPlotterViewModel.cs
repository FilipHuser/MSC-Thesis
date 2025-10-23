using System.Windows.Input;
using Graphium.Interfaces;
using ScottPlot;
using ScottPlot.MultiplotLayouts;
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
        public WpfPlot PlotControl { get; } = new WpfPlot();
        private readonly IMultiplot _multiplot;
        #endregion
        #region METHODS
        public DataPlotterViewModel(ISignalService signalService, ILoggingService loggingService)
        {
            _signalService = signalService;
            _loggingService = loggingService;
            _multiplot = PlotControl.Multiplot;
            Init();
        }
        private void Init() => _multiplot.RemovePlot(_multiplot.GetPlot(0)); 
        public void OnSignalsChanged()
        {
            PlotControl.Reset();
            _multiplot.RemovePlot(_multiplot.GetPlot(0));
            UnsubscribePlotEvents();

            var signals = _signalService.Signals?.SelectMany(x => x.GetSignals());

            if(signals == null || !signals.Any()) { return; }

            foreach (var signal in signals)
            {
                var plot = _multiplot.AddPlot();
                plot.Axes.Left.Label.Text = signal.Name;
                var scatter = plot.Add.Scatter(signal.X, signal.Y);
                scatter.Axes.YAxis = plot.Axes.Right;
            }

            _multiplot.CollapseVertically();
            var plots = _multiplot.GetPlots();
            var bottomPlot = plots.Last();

            foreach(var plot in plots)
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
        private void SubscribePlotEvents()
        {
            PlotControl.MouseDown += OnPlotMouseDown;
            PlotControl.MouseUp += OnPlotMouseUp;
            PlotControl.MouseMove += OnPlotMouseMove;
        }
        private void UnsubscribePlotEvents()
        {
            PlotControl.MouseDown -= OnPlotMouseDown;
            PlotControl.MouseUp -= OnPlotMouseUp;
            PlotControl.MouseMove -= OnPlotMouseMove;
        }
        private void OnPlotMouseDown(object s, MouseButtonEventArgs e) 
        {
            if (_layout == null) return; double mouseY = e.GetPosition(PlotControl).Y;
            _dividerBeingDragged = _layout.GetDivider((float)mouseY); 
            PlotControl.UserInputProcessor.IsEnabled = _dividerBeingDragged is null;
        }
        private void OnPlotMouseUp(object s, MouseButtonEventArgs e)
        {
            if (_dividerBeingDragged is not null)
            {
                _dividerBeingDragged = null;
                PlotControl.UserInputProcessor.IsEnabled = true;
            }
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
