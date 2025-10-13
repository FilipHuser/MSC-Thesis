using System.Collections.ObjectModel;
using System.IO;
using System.Net.Sockets;
using System.Security;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using DataHub;
using DataHub.Core;
using DataHub.Modules;
using Graphium.Controls;
using Graphium.Core;
using Graphium.Models;
using Microsoft.Win32;
using NLog.Layouts;
using ScottPlot;
using ScottPlot.MultiplotLayouts;
using ScottPlot.Plottables;
using ScottPlot.WPF;

namespace Graphium.ViewModels
{
    internal partial class MeasurementTabControlVM : ViewModelBase
    {
        #region PROPERTIES
        private readonly Hub _dh;
        private SignalStorage? _signalStorage;
        private CancellationTokenSource? _cts;
        private ObservableCollection<SignalBase> _signals = [];
        private Dictionary<ModuleType, int> _signalCounts = new Dictionary<ModuleType, int>();
        private DraggableRows? _layout;
        private int? _dividerBeingDragged;
        private bool _isMeasuring = false;
        private readonly object _measurementLock = new object();

        public string Title { get; set; }
        public UserControl Tab { get; set; }

        public bool IsMeasuring
        {
            get => _isMeasuring;
            set => SetProperty(ref _isMeasuring, value);
        }

        public WpfPlot PlotControl { get; set; } = new WpfPlot();

        public ObservableCollection<SignalBase> Signals
        {
            get => _signals;
            set
            {
                SetProperty(ref _signals, value);
                OnSignalsUpdate();
            }
        }

        public event Action? MeasurementStartRequested;

        #region RELAY_COMMANDS
        public RelayCommand StartMeasurementCmd => new RelayCommand(
            execute => StartMeasurement(),
            canExecute => Signals.Count > 0 && !IsMeasuring);

        public RelayCommand StopMeasurementCmd => new RelayCommand(
            execute => StopMeasurement(),
            canExecute => IsMeasuring);

        public RelayCommand SaveAsCSVCmd => new RelayCommand(
            execute => SaveAsCSV());
        #endregion
        #endregion

        public MeasurementTabControlVM(Window parent, string title, ref Hub dh) : base(parent)
        {
            _dh = dh;
            Title = title;
            Tab = new MeasurementTabControl(title);
            _signalStorage = new SignalStorage(this);
            Signals.CollectionChanged += (s, e) => { OnSignalsUpdate(); };
            Init();
        }

        #region METHODS
        private void Init()
        {
            var multiplot = PlotControl.Multiplot;
            multiplot.Reset();
            multiplot.RemovePlot(multiplot.GetPlot(0));
        }

        private async Task MeasurementLoop(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    Dictionary<ModuleType, Dictionary<int, List<object>>?> dataByModule;

                    lock (_measurementLock)
                    {
                        dataByModule = DataProcessor.ProcessAll(_dh.Modules.Values, _signalCounts);
                    }

                    if (dataByModule != null && dataByModule.Values.Any(v => v != null))
                    {
                        var dispatcher = Application.Current?.Dispatcher;
                        if (dispatcher != null)
                        {
                            await dispatcher.InvokeAsync(() =>
                            {
                                UpdateSignals(dataByModule);
                            });
                        }
                    }

                    await Task.Delay(16, token);
                }
            }
            catch (OperationCanceledException)
            {
                // Expected when cancellation is requested
            }
            catch (Exception ex)
            {
                Application.Current?.Dispatcher?.Invoke(() =>
                {
                    MessageBox.Show($"Measurement error: {ex.Message}",
                                    "Error",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                    StopMeasurement();
                });
            }
        }

        private void UpdateSignals(Dictionary<ModuleType, Dictionary<int, List<object>>?> dataByModule)
        {
            try
            {
                var moduleCounters = new Dictionary<ModuleType, int>();

                foreach (var signal in Signals)
                {
                    var sourceModuleType = signal.Source;

                    if (!dataByModule.ContainsKey(sourceModuleType)) { continue; }

                    var sourceData = dataByModule[sourceModuleType];
                    if (sourceData == null) continue;

                    int currentCounter = moduleCounters.TryGetValue(sourceModuleType, out int c) ? c : 0;

                    switch (signal)
                    {
                        case Models.Signal si:
                            if (!sourceData.TryGetValue(currentCounter, out var list)) break;
                            _signalStorage?.Add(si, list.First());
                            si.Update(new() { { 0, list } });
                            currentCounter++;
                            break;

                        case SignalComposite sc:
                            var slice = new Dictionary<int, List<object>>();
                            for (int i = 0; i < sc.Signals.Count; i++)
                            {
                                if (sourceData.TryGetValue(currentCounter + i, out var item))
                                    slice[i] = item;
                            }
                            _signalStorage?.Add(sc, slice);
                            sc.Update(slice);
                            currentCounter += sc.Signals.Count;
                            break;
                    }

                    moduleCounters[sourceModuleType] = currentCounter;
                }
            }
            catch (Exception ex)
            {
                // Log error but don't crash the measurement loop
                System.Diagnostics.Debug.WriteLine($"Error updating signals: {ex.Message}");
            }
        }

        public void StartMeasurement()
        {
            lock (_measurementLock)
            {
                if (IsMeasuring) return;

                MeasurementStartRequested?.Invoke();
                IsMeasuring = true;
                _dh.StartCapturing();
                _cts = new CancellationTokenSource();
                Task.Run(() => MeasurementLoop(_cts.Token));
            }
        }

        public void StopMeasurement()
        {
            lock (_measurementLock)
            {
                if (!IsMeasuring) return;

                IsMeasuring = false;
                _dh.StopCapturing();
                _cts?.Cancel();
                _cts?.Dispose();
                _cts = null;
            }
        }

        private void OnSignalsUpdate()
        {
            // Unsubscribe old event handlers to prevent memory leaks
            UnsubscribePlotEvents();

            PlotControl.Reset();
            var multiplot = PlotControl.Multiplot;
            multiplot.RemovePlot(multiplot.GetPlot(0));

            var signals = Signals.SelectMany(x => x.GetSignals()).ToList();

            foreach (var s in signals)
            {
                var plot = multiplot.AddPlot();
                plot.Axes.Left.Label.Text = s.Name;
                var scatter = plot.Add.Scatter(s.X, s.Y);
                scatter.Axes.YAxis = plot.Axes.Right;
            }

            multiplot.CollapseVertically();

            var plots = multiplot.GetPlots();
            var bottomPlot = plots.Last();

            foreach (var plot in plots)
            {
                plot.Axes.Left.LockSize(32);
                plot.Axes.Right.LockSize(64);

                plot.Grid.XAxis = bottomPlot.Axes.Bottom;
                plot.Grid.YAxis = plot.Axes.Right;
            }

            multiplot.SharedAxes.ShareX(multiplot.GetPlots());

            _layout = new DraggableRows();
            multiplot.Layout = _layout;
            _dividerBeingDragged = null;

            // Subscribe new event handlers
            SubscribePlotEvents();

            PlotControl.Refresh();
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
            if (_layout == null) return;

            double mouseY = e.GetPosition(PlotControl).Y;
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

            if (_dividerBeingDragged is not null)
            {
                double mouseY = e.GetPosition(PlotControl).Y;
                _layout.SetDivider(_dividerBeingDragged.Value, (float)mouseY);
                PlotControl.Refresh();
            }

            Mouse.OverrideCursor = _layout.GetDivider((float)e.GetPosition(PlotControl).Y) != null
                ? Cursors.SizeNS
                : Cursors.Arrow;
        }

        private void SaveAsCSV()
        {
            bool wasRunning = IsMeasuring;

            if (wasRunning)
            {
                StopMeasurement();
            }

            try
            {
                string sourceFile = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "Graphium",
                    "Measurements",
                    $"{Title}_tmpMeasurement.csv");

                if (!File.Exists(sourceFile))
                {
                    MessageBox.Show("No measurement file found.",
                                    "Error",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Warning);
                    return;
                }

                // Open save dialog for the user
                var dialog = new SaveFileDialog
                {
                    InitialDirectory = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)),
                    Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                    FileName = "measurement.csv"
                };

                if (dialog.ShowDialog() == true)
                {
                    File.Copy(sourceFile, dialog.FileName, overwrite: true);
                    MessageBox.Show("CSV file saved successfully.",
                                    "Success",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save CSV: {ex.Message}",
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
            finally
            {
                // Optionally restart measurement if it was running
                if (wasRunning)
                {
                    var result = MessageBox.Show("Resume measurement?",
                                                  "Continue",
                                                  MessageBoxButton.YesNo,
                                                  MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        StartMeasurement();
                    }
                }
            }
        }
        #endregion
    }
}