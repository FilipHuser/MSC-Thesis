using DataHub;
using DataHub.Interfaces;
using DataHub.Modules;
using Graphium.Interfaces;

namespace Graphium.Services
{
    class DataHubService : IDataHubService
    {
        #region PROPERTIES
        private readonly Hub _hub;
        #endregion
        #region METHODS
        public DataHubService()
        {
            _hub = new Hub();
            var getAppSetting = (string key) => System.Configuration.ConfigurationManager.AppSettings[key];
            int.TryParse(getAppSetting("CaptureDeviceIndex"), out int captureDeviceIndex);
            int.TryParse(getAppSetting("PayloadSize"), out int payloadSize);
            var ipAddr = getAppSetting("IPAddr");

            string filter = $"udp and src host {ipAddr} and udp[4:2] > {payloadSize}";

            var packetModule = new BiopacSourceModule(captureDeviceIndex, filter, 5);
            var httpModule = new VRSourceModule(getAppSetting("URI"));

            _hub.AddModule(packetModule);
            _hub.AddModule(httpModule);
        }
        public void StartCapturing() => _hub.StartCapturing();
        public void StopCapturing() => _hub.StopCapturing();
        public void AddModule(IModule module) => _hub.AddModule(module);
        public void RemoveModule(IModule module) => _hub.RemoveModule(module);
        #endregion
    }
}
