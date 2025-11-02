using DataHub;
using DataHub.Core;
using DataHub.Interfaces;
using DataHub.Modules;
using Graphium.Core;
using Graphium.Interfaces;
using Graphium.Models;

namespace Graphium.Services
{
    class DataHubService : IDataHubService
    {
        #region SERVICES
        private readonly ISignalService _signalService;
        private readonly IAppConfigurationService _appConfigurationService;
        private readonly ILoggingService _loggingService;
        #endregion
        #region PROPERTIES
        private readonly Hub _hub;
        private Dictionary<ModuleType, int> _signalCounts = [];
        public bool IsCapturing => _hub.IsCapturing;
        #endregion
        #region METHODS
        public DataHubService(ISignalService signalService, IConfigurationService ConfigurationService, IAppConfigurationService appConfigurationService, ILoggingService loggingService)
        {
            _hub = new Hub();
            _signalService = signalService;
            _appConfigurationService = appConfigurationService;
            _loggingService = loggingService;
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
            _loggingService.LogInfo($"Initializing modules with CaptureDevice: {settings.CaptureDeviceIndex}, IP: {settings.IPAddr}");
            foreach (var module in _hub.Modules.Values.ToList())
            {
                _loggingService.LogDebug($"Removing module: {module.GetType().Name}");
                _hub.RemoveModule(module);

                if (module is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }

            int captureDeviceIndex = settings.CaptureDeviceIndex;
            int payloadSize = settings.PayloadSize;
            string ipAddr = settings.IPAddr;
            string uri = settings.URI ?? "http://localhost:8888/";
            string filter = $"udp and src host {ipAddr} and udp[4:2] > {payloadSize}";

            try
            {
                var packetModule = new BiopacSourceModule(captureDeviceIndex, filter, 5);
                var httpModule = new VRSourceModule(uri);

                _hub.AddModule(packetModule);
                _hub.AddModule(httpModule);

                _loggingService.LogInfo("Modules initialized successfully");
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Failed to initialize modules: {ex.Message}");
                throw;
            }
        }
        public Dictionary<ModuleType, Dictionary<int, List<object>>?> GetData() => DataProcessor.ProcessAll(_hub.Modules.Values, _signalCounts);
        public void StartCapturing()
        {
            _hub.StartCapturing();
            _loggingService.LogInfo("Starting data capture");
        }
        public void StopCapturing()
        {
            _hub.StopCapturing();
            _loggingService.LogInfo("Stopping data capture");
        }
        public void AddModule(IModule module)
        {
            _hub.AddModule(module);
            _loggingService.LogInfo($"Adding module: {module.GetType().Name}");
        }
        public void RemoveModule(IModule module)
        {
            _hub.RemoveModule(module);
            _loggingService.LogInfo($"Removing module: {module.GetType().Name}");
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
            _loggingService.LogInfo("Configuration changed, reinitializing modules");
            bool wasCapturing = _hub.IsCapturing;

            try
            {
                if (wasCapturing)
                {
                    _loggingService.LogInfo("Stopping capture before reconfiguration");
                    _hub.StopCapturing();
                }

                System.Threading.Thread.Sleep(100);
                AppSettings settings = _appConfigurationService.GetAppSettings();
                InitializeModules(settings);
                if (wasCapturing)
                {
                    _loggingService.LogInfo("Restarting capture after reconfiguration");
                    _hub.StartCapturing();
                }

                _loggingService.LogInfo("Configuration change completed successfully");
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error reinitializing modules: {ex.Message}");
            }
        }
        #endregion
    }
}