using System.Collections.ObjectModel;
using Graphium.Models;
using System.Windows;
using DataHub;
using DataHub.Modules;
using System.Configuration;
using Graphium.Core;
using System.Windows.Threading;
using ScottPlot.WPF;
using ScottPlot;
using SharpPcap;

namespace Graphium.ViewModels
{
    internal class MonitorViewModel : BaseViewModel
    {
        #region PROPERTIES
        private readonly DataHub.Hub _hub;
        private DispatcherTimer _updateTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(10),
            IsEnabled = true
        };
        private DispatcherTimer _dataTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(10),
            IsEnabled = true
        };
        public WpfPlot PlotControl { get; } = new WpfPlot();
        public ObservableCollection<SignalBase> Signals { get; set; } = new ObservableCollection<SignalBase>();

        #endregion
        #region METHODS
        public MonitorViewModel(Window window , ObservableCollection<SignalBase> signals) : base(window)
        {
            Signals = signals;

            _hub = new DataHub.Hub();
            var getAppSetting = (string key) => ConfigurationManager.AppSettings[key];

            int.TryParse(getAppSetting("CaptureDeviceIndex"), out int captureDeviceIndex);
            int.TryParse(getAppSetting("PayloadSize"), out int payloadSize);
            var ipAddr = getAppSetting("IPAddr");

            string filter = $"udp and src host {ipAddr} and udp[4:2] > {payloadSize}";

            var packetModule = new PacketModule(captureDeviceIndex , filter , 5);
            var httpModule = new HTTPModule<string>(getAppSetting("URI"));


            _hub.AddModule(packetModule);
            _hub.AddModule(httpModule);
            _hub.StartCapturing();


            PlotControl.Multiplot.RemovePlot(PlotControl.Multiplot.GetPlot(0));
            foreach (var signal in Signals)
            {
                switch(signal)
                {
                    case Signal s:
                        PlotControl.Multiplot.AddPlot(s.Plot);
                        break;
                    case SignalComposite sc:
                        sc.Plots.ForEach(x => PlotControl.Multiplot.AddPlot(x));
                        break;
                }
            }
            PlotControl.Multiplot.CollapseVertically();

            UpdateData();
            Window.Closing += (e, s) => { _hub.StopCapturing(); };
        }
        private void UpdateData()
        {
            var storage = new SignalStorage(Signals.ToList());

            _updateTimer.Tick += (s, e) =>
            {
                PlotControl.Refresh();
            };

            _dataTimer.Tick += (s, e) =>
            {
                var packetModule = (PacketModule)_hub.Modules[typeof(PacketModule)];
                var httpModule = (HTTPModule<string>)_hub.Modules[typeof(HTTPModule<string>)];

                var packetModuleSignals = Signals.Where(x => x.Source == packetModule.GetType());
                var httpModuleSignals = Signals.Where(x => x.Source == httpModule.GetType());

                var packetData = DataProcessor.Process(packetModule, packetModuleSignals.Sum(x => x.Count));
                var httpData = DataProcessor.Process(httpModule, httpModuleSignals.Sum(x => x.Count));


                foreach (var signal in Signals)
                {
                    Dictionary<int, List<object>> data = null;

                    // Select corresponding data source
                    if (packetModuleSignals.Contains(signal))
                    {
                        if (packetData.Count == 0) continue;
                        data = packetData;
                    }
                    else if (httpModuleSignals.Contains(signal))
                    {
                        if (httpData.Count == 0) continue;
                        data = httpData;
                    }

                    if (data != null)
                    {
                        // Update the signal (adds data to streams)
                        signal.Update(data);

                        // For each index/value pair in data, add to storage
                        // But SignalStorage keys on Signal, so for SignalComposite you might want to add each sub-Signal's value separately
                        switch (signal)
                        {
                            case Signal si:
                                // Add aggregated value for this Signal (e.g. first value of index 0)
                                if (data.TryGetValue(0, out var values) && values.Count > 0)
                                {
                                    storage.Add(si, values[0]);
                                }
                                break;

                            case SignalComposite sc:
                                // For composite, add each inner Signal and its corresponding value
                                foreach (var kvp in data)
                                {
                                    int idx = kvp.Key;
                                    if (idx >= 0 && idx < sc.Signals.Count)
                                    {
                                        var innerSignal = sc.Signals[idx];
                                        var listValues = kvp.Value;
                                        if (listValues.Count > 0)
                                        {
                                            storage.Add(innerSignal, listValues[0]);
                                        }
                                    }
                                }
                                break;
                        }
                    }
                }
            };
        }
        #endregion
    }
}


/*
 
var signal1 = new Signal(typeof(PacketModule));
var signal2 = new Signal(typeof(PacketModule));
var signal3 = new Signal(typeof(PacketModule));
var signal4 = new Signal(typeof(PacketModule));

var signalCompound = new SignalComposite(typeof(PacketModule), new(){ signal2, signal3, signal4 });


var list = new List<SignalBase>() { signal1, signalCompound };

            
var testData = new Dictionary<int, List<double>>
{
    { 0, new List<double> { 1.0, 2.0, 3.0, 4.0, 5.0 } },
    { 1, new List<double> { 10.0, 9.0, 8.0, 7.0, 6.0 } },
    { 2, new List<double> { 10.0, 9.0, 8.0, 7.0, 6.0 } },
    { 3, new List<double> { 0.5, 1.5, 2.5, 3.5, 4.5 } }
};
 
 */