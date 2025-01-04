using FHAPILib;
using PacketDotNet;
using SharpPcap;

namespace FHMonitor.Services
{
    public class FHAPIService
    {
        public FHAPIService()
        {
            FHAPI = new FHAPI();
            NPacketsPerFetch = 1;
        }
        #region PROPERTIES
        public FHAPI FHAPI { get; set; }
        public int NPacketsPerFetch {  get; set; }
        public int PacketCount => FHAPI.CapturedPackets.Count;
        #endregion
        #region METHODS
        public void StartCapturing(int deviceIndex) => FHAPI.StartCapturing(deviceIndex);
        public void StopCapturing() => FHAPI.StopCapturing();
        public List<RawCapture> GetPackets()
        {
            List<RawCapture> packets = new List<RawCapture>();

            for (int i = 0; i < NPacketsPerFetch; i++)
            {
                if (FHAPI.CapturedPackets.TryDequeue(out RawCapture? packet))
                {
                    if (packet != null) { packets.Add(packet); }
                }
                else
                {
                    break;
                }
            }

            return packets;
        }
        #endregion
    }
}
