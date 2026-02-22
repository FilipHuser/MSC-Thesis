using Graphium.Enums;
using Graphium.Interfaces;
using Graphium.Models;
using ScottPlot;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Graphium.ViewModels
{
    internal class DataPlotterViewModel : ViewModelBase
    {
        #region SERVICES
        private readonly ILoggingService _loggingService;
        private readonly IViewModelFactory _viewModelFactory;
        #endregion
        #region PROPERTIES
        private double _xMin = 0;
        private double _xMax = 1;
        private double _dataAreaLeft;
        private double _dataAreaRight;
        private bool _paletteApplied = false;
        private const int RefreshIntervalMs = 16;
        private string _selectedPalette = "Category10";
        private static readonly Dictionary<string, IPalette> Palettes = new()
        {
            ["Category10"] = new ScottPlot.Palettes.Category10(),
            ["Nord"] = new ScottPlot.Palettes.Nord(),
            ["Frost"] = new ScottPlot.Palettes.Frost(),
            ["Amber"] = new ScottPlot.Palettes.Amber(),
            ["Aurora"] = new ScottPlot.Palettes.Aurora(),
        };

        public string SelectedPalette
        {
            get => _selectedPalette;
            set
            {
                SetProperty(ref _selectedPalette, value);
                _paletteApplied = false;
                System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    ApplyPalette();
                });
            }
        }
        private DateTime _lastRefresh = DateTime.MinValue;
        public IEnumerable<string> AvailablePalettes => Palettes.Keys;
        public double XMin { get => _xMin; set => SetProperty(ref _xMin, value); }
        public double XMax { get => _xMax; set => SetProperty(ref _xMax, value); }
        public double DataAreaLeft { get => _dataAreaLeft; set => SetProperty(ref _dataAreaLeft, value); }
        public double DataAreaRight { get => _dataAreaRight; set => SetProperty(ref _dataAreaRight, value); }
        public ObservableCollection<ISignalVisualizerViewModel> Visualizers { get; set; } = [];
        #endregion
        #region METHODS
        public DataPlotterViewModel(ILoggingService loggingService, IViewModelFactory viewModelFactory)
        {
            _loggingService = loggingService;
            _viewModelFactory = viewModelFactory;
        }

        public void OnSignalsChanged(IReadOnlyCollection<SignalBase>? signals)
        {
            Visualizers.Clear();
            _paletteApplied = false;
            if (signals == null) return;

            foreach (var signal in signals)
            {
                if (signal == null) continue;
                ISignalVisualizerViewModel visualizer = signal switch
                {
                    NumericSignal numeric => new NumericSignalViewModel(numeric),
                    TextSignal text => new TextSignalViewModel(text),
                    _ when signal.Type == SignalType.NaN => throw new InvalidOperationException($"Signal '{signal.Name}' has no type set."),
                    _ => throw new NotSupportedException($"SignalType {signal.Type} is not supported")
                };
                Visualizers.Add(visualizer);
            }

            ApplySharedXAxis();
        }
        public void Update(double xVal)
        {
            var now = DateTime.Now;
            if ((now - _lastRefresh).TotalMilliseconds < RefreshIntervalMs) return;
            _lastRefresh = now;

            System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
            {
                foreach (var vm in Visualizers)
                    vm.Refresh();

                if (!_paletteApplied)
                {
                    ApplyPalette();
                    _paletteApplied = true;
                }
            }, System.Windows.Threading.DispatcherPriority.Background);
        }

        private void ApplyPalette()
        {
            var palette = Palettes[_selectedPalette];
            var numericVMs = Visualizers.OfType<NumericSignalViewModel>().ToList();
            for (int i = 0; i < numericVMs.Count; i++)
                numericVMs[i].SetColor(palette.GetColor(i));
        }

        private void ApplySharedXAxis()
        {
            var numericVMs = Visualizers.OfType<NumericSignalViewModel>().ToList();
            if (numericVMs.Count == 0) return;

            foreach (var vm in numericVMs)
            {
                void SyncFromVm()
                {
                    var limits = vm.PlotControl.Plot.Axes.GetLimits();
                    XMin = limits.Left;
                    XMax = limits.Right;
                    foreach (var other in numericVMs.Where(x => x != vm))
                    {
                        other.PlotControl.Plot.Axes.SetLimitsX(limits.Left, limits.Right);
                        other.PlotControl.Refresh();
                    }
                }

                vm.PlotControl.MouseMove += (_, e) =>
                {
                    if (e.LeftButton != MouseButtonState.Pressed) return;
                    SyncFromVm();
                };
                vm.PlotControl.MouseUp += (_, _) => SyncFromVm();
                vm.PlotControl.PreviewMouseWheel += (_, _) =>
                    vm.PlotControl.Dispatcher.BeginInvoke(SyncFromVm,
                        System.Windows.Threading.DispatcherPriority.Background);

                vm.PlotControl.Dispatcher.BeginInvoke(() =>
                {
                    vm.PlotControl.Refresh();
                    SyncFromVm();
                }, System.Windows.Threading.DispatcherPriority.Loaded);
            }
        }
        #endregion
    }
}