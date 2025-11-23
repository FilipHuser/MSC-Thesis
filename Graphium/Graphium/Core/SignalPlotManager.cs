using ScottPlot;
using ScottPlot.Plottables;
using Graphium.Models;

namespace Graphium.Services
{
    public class SignalPlotManager
    {
        private readonly Dictionary<Models.Signal, PlotData> _plotDataMap = new();
        private readonly IPalette _palette;
        private class PlotData
        {
            public Plot Plot { get; set; } = new();
            public List<SignalXY> Plottables { get; set; } = new();
        }
        public SignalPlotManager(IPalette? palette = null)
        {
            _palette = palette ?? new ScottPlot.Palettes.Nord();
        }
        public IEnumerable<Plot> GetAllPlots() => _plotDataMap.Values.Select(pd => pd.Plot);
        public Plot GetOrCreatePlot(Models.Signal signal)
        {
            if (!_plotDataMap.ContainsKey(signal))
            {
                var plotData = new PlotData();
                _plotDataMap[signal] = plotData;
                InitializePlot(signal, plotData);
            }

            return _plotDataMap[signal].Plot;
        }
        private void InitializePlot(Models.Signal signal, PlotData plotData)
        {
            for (int i = 0; i < signal.YData.Count; i++)
            {
                AddChannelToPlot(signal, plotData, i);
            }
        }
        private void AddChannelToPlot(Models.Signal signal, PlotData plotData, int channelIndex)
        {
            var signalXY = plotData.Plot.Add.SignalXY(
                signal.XData,
                signal.YData[channelIndex]
            );

            signalXY.LegendText = $"{signal.Name} CH{channelIndex + 1}";

            signalXY.LineWidth = 2;
            signalXY.Color = _palette.GetColor(channelIndex);
            signalXY.Axes.YAxis = plotData.Plot.Axes.Right;
            plotData.Plottables.Add(signalXY);
        }
        public void UpdatePlot(Models.Signal signal)
        {
            if (!_plotDataMap.ContainsKey(signal)) return;

            var plotData = _plotDataMap[signal];

            while (plotData.Plottables.Count < signal.YData.Count)
            {
                AddChannelToPlot(signal, plotData, plotData.Plottables.Count);
            }
        }
        public void ClearAll()
        {
            foreach (var plotData in _plotDataMap.Values)
            {
                foreach (var plottable in plotData.Plottables)
                {
                    plotData.Plot.Remove(plottable);
                }
                plotData.Plottables.Clear();
                plotData.Plot.Clear();
            }
            _plotDataMap.Clear();
        }
        public void SetChannelColor(Models.Signal signal, int channelIndex, Color color)
        {
            if (!_plotDataMap.ContainsKey(signal)) return;

            var plotData = _plotDataMap[signal];
            if (channelIndex < plotData.Plottables.Count)
            {
                plotData.Plottables[channelIndex].Color = color;
            }
        }
        public void SetAllChannelsColor(Models.Signal signal, Color color)
        {
            if (!_plotDataMap.ContainsKey(signal)) return;

            var plotData = _plotDataMap[signal];
            foreach (var plottable in plotData.Plottables)
            {
                plottable.Color = color;
            }
        }
        public void ClearPlot(Models.Signal signal)
        {
            if (!_plotDataMap.ContainsKey(signal)) return;

            var plotData = _plotDataMap[signal];

            foreach (var plottable in plotData.Plottables)
            {
                plotData.Plot.Remove(plottable);
            }

            plotData.Plottables.Clear();

            // Reinitialize
            InitializePlot(signal, plotData);
        }
        public void RemoveSignal(Models.Signal signal)
        {
            if (_plotDataMap.ContainsKey(signal))
            {
                var plotData = _plotDataMap[signal];
                foreach (var plottable in plotData.Plottables)
                {
                    plotData.Plot.Remove(plottable);
                }
                _plotDataMap.Remove(signal);
            }
        }
        public List<SignalXY> GetPlottables(Models.Signal signal)
        {
            return _plotDataMap.ContainsKey(signal)
                ? _plotDataMap[signal].Plottables
                : new List<SignalXY>();
        }
    }
}