using FHAPI.Core;
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
        public List<FHPacket> GetPackets() => _fhapi.GetPackets();
        public void StartCapturing() => _fhapi.StartCapturing();
        public void StopCapturing() => _fhapi.StopCapturing();
        public void SetSetting(MonitorSettingsViewModel ms)
        {
            _fhapi.SetDeviceIndex(ms.CaptureDeviceIndex);
            _fhapi.SetFilter(ms.Filter??"");
        }
        #endregion
    }
}
