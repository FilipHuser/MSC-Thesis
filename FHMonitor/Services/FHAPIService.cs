using FHAPILib;
using FHMonitor.Models.ViewModels;
using PacketDotNet;
using SharpPcap;

namespace FHMonitor.Services
{
    public class FHAPIService
    {
        #region PROPERTIES
        private readonly FHAPILib.FHAPI _fhapi;
        #endregion
        public FHAPIService(FHAPILib.FHAPI fhapi)
        {
            _fhapi = fhapi;
        }
        #region METHODS
        public CaptureDeviceList GetCaptureDevices() => _fhapi.CaptureDevices;
        public void SetSetting(MonitorSettingsViewModel ms)
        {
            _fhapi.SetDeviceIndex(ms.CaptureDeviceIndex);
            _fhapi.SetFilter(ms.Filter??"");
        }
        #endregion
    }
}
