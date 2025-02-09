using FHAPILib;
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
        #endregion
    }
}
