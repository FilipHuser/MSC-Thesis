using FHAPILib;
using PacketDotNet;
using SharpPcap;

namespace FHMonitor.Services
{
    public class FHAPIService
    {
        public FHAPIService()
        {
            Console.WriteLine("Init");
            FHAPI = new FHAPI();
            FHAPI.Capture(4);
        }
        #region PROPERTIES
        private FHAPI FHAPI { get; set; }
        public int PacketCount => FHAPI.CapturedPackets.Count;
        #endregion
        #region METHODS
        public List<RawCapture> GetPackets(int nPackets)
        {
            List<RawCapture> packets = new List<RawCapture>();

            for (int i = 0; i < nPackets; i++)
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
