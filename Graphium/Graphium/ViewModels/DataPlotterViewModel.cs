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
        private double _currentXMax = 0;
        private bool _isFollowing = false;
        private bool _paletteApplied = false;
        private const double FollowSmoothingFactor = 0.1;
        private Task? _renderTask;
        private CancellationTokenSource? _renderCts;
        private PeriodicTimer? _renderTimer;
        private string _selectedPalette = "Category10";
        public double ViewWindowMs { get; set; } = 5000;
        public bool IsFollowing
        {
            get => _isFollowing;
            set => SetProperty(ref _isFollowing, value);
        }
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
                _ = System.Windows.Application.Current.Dispatcher.InvokeAsync(ApplyPalette);
            }
        }
        public IEnumerable<string> AvailablePalettes => Palettes.Keys;
        public double XMin { get => _xMin; set => SetProperty(ref _xMin, value); }
        public double XMax { get => _xMax; set => SetProperty(ref _xMax, value); }
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
            int colorIndex = 0;
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
                if (visualizer is NumericSignalViewModel numVm)
                    numVm.SetColor(Palettes[_selectedPalette].GetColor(colorIndex++));
                Visualizers.Add(visualizer);
            }
            ApplySharedXAxis();
        }
        public void ToggleFollow() => IsFollowing = !IsFollowing;
        public void StartRendering()
        {
            IsFollowing = true;
            _paletteApplied = false;
            _currentXMax = 0;
            _renderCts = new CancellationTokenSource();
            _renderTimer = new PeriodicTimer(TimeSpan.FromMilliseconds(33));
            _renderTask = RunRenderLoopAsync(_renderCts.Token);
        }
        public void StopRendering()
        {
            _renderCts?.Cancel();
            _renderCts?.Dispose();
            _renderCts = null;
            _renderTimer?.Dispose();
            _renderTimer = null;
            _renderTask = null;
        }
        private async Task RunRenderLoopAsync(CancellationToken token)
        {
            try
            {
                while (await _renderTimer!.WaitForNextTickAsync(token))
                {
                    _ = System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
                    {
                        foreach (var vm in Visualizers)
                            vm.Refresh();

                        var numericVMs = Visualizers.OfType<NumericSignalViewModel>().ToList();

                        if (numericVMs.Count > 1)
                        {
                            var masterLimits = numericVMs[0].PlotControl.Plot.Axes.GetLimits();
                            foreach (var vm in numericVMs.Skip(1).Where(v => v.NeedsXSync))
                            {
                                vm.PlotControl.Plot.Axes.SetLimitsX(masterLimits.Left, masterLimits.Right);
                                vm.NeedsXSync = false;
                            }
                        }

                        if (_isFollowing)
                        {
                            var xData = numericVMs.FirstOrDefault()?.SignalBase.XData;
                            if (xData?.Count > 0)
                            {
                                double targetXMax = xData[^1];
                                if (_currentXMax == 0)
                                    _currentXMax = targetXMax;
                                else
                                    _currentXMax += (targetXMax - _currentXMax) * FollowSmoothingFactor;

                                foreach (var vm in numericVMs)
                                {
                                    vm.AutoScaleY();
                                    vm.ScrollTo(_currentXMax, ViewWindowMs);
                                }

                                XMin = _currentXMax - ViewWindowMs;
                                XMax = _currentXMax;
                            }
                        }

                        if (!_paletteApplied && numericVMs.All(vm => vm.HasPlottables))
                        {
                            ApplyPalette();
                            _paletteApplied = true;
                        }
                    }, System.Windows.Threading.DispatcherPriority.Background);
                }
            }
            catch (OperationCanceledException) { }
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
                    if (_isFollowing) return;
                    var limits = vm.PlotControl.Plot.Axes.GetLimits();
                    XMin = limits.Left;
                    XMax = limits.Right;
                    foreach (var other in numericVMs.Where(x => x != vm))
                    {
                        other.PlotControl.Plot.Axes.SetLimitsX(limits.Left, limits.Right);
                        other.PlotControl.Refresh();
                    }
                    foreach (var tvm in Visualizers.OfType<TextSignalViewModel>())
                        tvm.Refresh();
                }
                vm.PlotControl.MouseDown += (_, _) => IsFollowing = false;
                vm.PlotControl.MouseMove += (_, e) =>
                {
                    if (e.LeftButton != MouseButtonState.Pressed && e.RightButton != MouseButtonState.Pressed) return;
                    SyncFromVm();
                };
                vm.PlotControl.MouseUp += (_, _) => SyncFromVm();
                vm.PlotControl.PreviewMouseWheel += (_, _) =>
                    _ = vm.PlotControl.Dispatcher.BeginInvoke(SyncFromVm, System.Windows.Threading.DispatcherPriority.Render);
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