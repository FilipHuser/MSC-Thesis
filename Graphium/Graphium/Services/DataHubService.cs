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
        #endregion
        #region PROPERTIES
        private readonly Hub _hub;
        private Dictionary<ModuleType, int> _signalCounts = [];
        public bool IsCapturing => _hub.IsCapturing;
        #endregion
        #region METHODS
        public DataHubService(ISignalService signalService, ISettingsService settingsService)
        {
            _hub = new Hub();
            _signalService = signalService;
            Init();
        }
        public void Init()
        {
            //TBD => SETTINGS SERVICE
            var getAppSetting = (string key) => System.Configuration.ConfigurationManager.AppSettings[key];
            int.TryParse(getAppSetting("CaptureDeviceIndex"), out int captureDeviceIndex);
            int.TryParse(getAppSetting("PayloadSize"), out int payloadSize);
            var ipAddr = getAppSetting("IPAddr");
            string filter = $"udp and src host {ipAddr} and udp[4:2] > {payloadSize}";
            var packetModule = new BiopacSourceModule(captureDeviceIndex, filter, 5);
            var httpModule = new VRSourceModule(getAppSetting("URI"));

            _signalService.SignalsChanged += UpdateSignalCounts;

            _hub.AddModule(packetModule);
            _hub.AddModule(httpModule);
        }
        //TBD => TRY CATCH BLOCKS
        public Dictionary<ModuleType, Dictionary<int, List<object>>?> GetData() => DataProcessor.ProcessAll(_hub.Modules.Values, _signalCounts);
        public void StartCapturing() => _hub.StartCapturing();
        public void StopCapturing() => _hub.StopCapturing();
        public void AddModule(IModule module) => _hub.AddModule(module);
        public void RemoveModule(IModule module) => _hub.RemoveModule(module);
        private void UpdateSignalCounts(object? sernder , EventArgs e)
        {
            _signalCounts = _signalService.Signals?
                .SelectMany(x => x.GetSignals())
                .GroupBy(x => x.Source)
                .ToDictionary(g => g.Key, g => g.Count())
                ?? new Dictionary<ModuleType, int>();
        }
        #endregion
    }
}
