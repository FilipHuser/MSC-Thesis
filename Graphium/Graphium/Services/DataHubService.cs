using DataHub;
using DataHub.Core;
using DataHub.Interfaces;
using DataHub.Modules;
using Graphium.Core;
using Graphium.Interfaces;
using Graphium.Models;
using SharpPcap;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Graphium.Services
{
    class DataHubService : IDataHubService
    {
        #region SERVICES
        private readonly ISignalService _signalService;
        private readonly IAppConfigurationService _appConfigurationService;
        private readonly ILoggingService _loggingService;
        private readonly IDialogService _dialogService;
        #endregion
        #region PROPERTIES
        private readonly Hub _hub;
        // _signalCounts is kept for UpdateSignalCounts event handler
        private Dictionary<ModuleType, int> _signalCounts = [];
        public bool IsCapturing => _hub.IsCapturing;
        #endregion
        #region METHODS
        public DataHubService(ISignalService signalService,
                              IConfigurationService ConfigurationService,
                              IAppConfigurationService appConfigurationService,
                              ILoggingService loggingService,
                              IDialogService dialogService)
        {
            _hub = new Hub();
            _signalService = signalService;
            _appConfigurationService = appConfigurationService;
            _loggingService = loggingService;
            _dialogService = dialogService;
            Init();
        }

        public void Init()
        {
            _loggingService.LogInfo("Initializing DataHubService");
            AppSettings settings = _appConfigurationService.GetAppSettings();
            InitializeModules(settings);
            _appConfigurationService.ConfigurationChanged += OnConfigurationChanged;
            _signalService.SignalsChanged += UpdateSignalCounts;
        }

        private void InitializeModules(AppSettings settings)
        {
            _loggingService.LogDebug($"Initializing modules with CaptureDevice: {settings.CaptureDeviceIndex}, IP: {settings.IPAddr}");

            foreach (var module in _hub.Modules.Values.ToList())
            {
                _loggingService.LogDebug($"Removing module: {module.GetType().Name}");
                _hub.RemoveModule(module);

                if (module is IDisposable disposable)
                    disposable.Dispose();
            }

            int captureDeviceIndex = settings.CaptureDeviceIndex;
            int payloadSize = settings.PayloadSize;
            string ipAddr = settings.IPAddr;
            string uri = settings.URI ?? "http://localhost:8888/";
            string filter = $"udp and src host {ipAddr} and udp[4:2] >  {payloadSize}";

            try
            {
                var availableDevices = CaptureDeviceList.Instance;
                if (availableDevices == null || availableDevices.Count == 0)
                {
                    _loggingService.LogError("No capture devices detected!");
                    _dialogService.ShowWarning("No capture devices were found. Please check your hardware connections.");
                    return;
                }

                bool settingsChanged = false;

                if (captureDeviceIndex < 0 || captureDeviceIndex >= availableDevices.Count)
                {
                    string warningMessage =
                        $"The previously selected capture device (index {captureDeviceIndex}) is no longer available.\n\n" +
                        $"Found {availableDevices.Count} available device(s). The first device (index 0) will be used instead.";

                    _loggingService.LogWarning(warningMessage);
                    _dialogService.ShowWarning(warningMessage);

                    captureDeviceIndex = 0;
                    settings.CaptureDeviceIndex = 0;
                    settingsChanged = true;
                }

                if (settingsChanged)
                {
                    _loggingService.LogDebug("App settings changed; saving updated configuration.");
                    _appConfigurationService.SetAppSettings(settings);
                }

                var packetModule = new BiopacSourceModule(captureDeviceIndex, filter, 5);
                var httpModule = new VRSourceModule(uri);

                _hub.AddModule(packetModule);
                _hub.AddModule(httpModule);

                _loggingService.LogDebug("Modules initialized successfully");
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Failed to initialize modules: {ex.Message}");
                _dialogService.ShowWarning($"Failed to initialize modules:\n{ex.Message}");
                throw;
            }
        }

        public Dictionary<ModuleType, Dictionary<int, List<(object value, DateTime timestamp)>>?> GetData()
        {
            // FIX: Pass the Signals collection to the DataProcessor
            // This is required for name-based filtering of VR data.
            return DataProcessor.ProcessAll(_hub.Modules.Values, _signalService.Signals);
        }

        public void StartCapturing()
        {
            _hub.StartCapturing();
            _loggingService.LogDebug("Starting data capture");
        }

        public void StopCapturing()
        {
            _hub.StopCapturing();
            _loggingService.LogDebug("Stopping data capture");
        }

        public void AddModule(IModule module)
        {
            _hub.AddModule(module);
            _loggingService.LogDebug($"Adding module: {module.GetType().Name}");
        }

        public void RemoveModule(IModule module)
        {
            _hub.RemoveModule(module);
            _loggingService.LogDebug($"Removing module: {module.GetType().Name}");
        }

        private void UpdateSignalCounts(object? sender, EventArgs e)
        {
            _signalCounts = _signalService.Signals?
                .GroupBy(x => x.Source)
                .ToDictionary(g => g.Key, g => g.Count())
                ?? new Dictionary<ModuleType, int>();

            _loggingService.LogDebug($"Signal counts updated: {string.Join(", ", _signalCounts.Select(x => $"{x.Key}={x.Value}"))}");
        }

        private void OnConfigurationChanged(object? sender, EventArgs e)
        {
            _loggingService.LogDebug("Configuration changed, reinitializing modules");
            bool wasCapturing = _hub.IsCapturing;

            try
            {
                if (wasCapturing)
                {
                    _loggingService.LogDebug("Stopping capture before reconfiguration");
                    _hub.StopCapturing();
                }

                System.Threading.Thread.Sleep(100);
                AppSettings settings = _appConfigurationService.GetAppSettings();
                InitializeModules(settings);
                if (wasCapturing)
                {
                    _loggingService.LogDebug("Restarting capture after reconfiguration");
                    _hub.StartCapturing();
                }

                _loggingService.LogDebug("Configuration change completed successfully");
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error reinitializing modules: {ex.Message}");
            }
        }
        #endregion
    }
}