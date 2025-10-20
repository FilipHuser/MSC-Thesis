using Graphium.Interfaces;
using ScottPlot;
using ScottPlot.MultiplotLayouts;
using ScottPlot.WPF;

namespace Graphium.ViewModels
{
    internal class DataPlotterViewModel : ViewModelBase
    {
        #region PROPERTIES
        private readonly ISignalService _signalService;
        private DraggableRows? _layout;
        private bool? _dividerBeingDragged;
        public WpfPlot PlotControl { get; } = new WpfPlot();
        private readonly IMultiplot _multiplot;
        #endregion
        #region METHODS
        public DataPlotterViewModel(ISignalService signalService)
        {
            _signalService = signalService;
            _multiplot = PlotControl.Multiplot;
            Init();
        }
        private void Init() => _multiplot.RemovePlot(_multiplot.GetPlot(0)); 
        public void OnSignalsChanged()
        {
            PlotControl.Reset();
            _multiplot.RemovePlot(_multiplot.GetPlot(0));

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
            PlotControl.Refresh();

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

            PlotControl.Refresh();
        }

        #endregion
    }
}
