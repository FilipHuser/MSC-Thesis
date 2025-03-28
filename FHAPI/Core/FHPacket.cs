using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using FHAPILib;
using PacketDotNet;
using SharpPcap;

namespace FHAPI.Core
{
    public class FHPacket
    {
        public enum PacketSource 
        {
            BIOPAC,
            EMOTIV,
            OTHER
        }
        public FHPacket(RawCapture packet)
        {
            var parsedPacket = Packet.ParsePacket(packet.LinkLayerType, packet.Data);
            if (parsedPacket is EthernetPacket ethPacket && ethPacket.PayloadPacket is IPv4Packet ipPacket &&
                ipPacket.PayloadPacket is UdpPacket udpPacket)
            {
                SourceAddress = ipPacket.SourceAddress;
                DestinationAddress = ipPacket.DestinationAddress;
                Payload = udpPacket.PayloadData;
                Timestamp = packet.Timeval.Date;

                var convertFunc = Converter<sbyte>.GetPayloadConvertFunction();

                Source = convertFunc(Payload.Take(1).ToArray() , 0) switch
                {
                    1 => PacketSource.BIOPAC,
                    2 => PacketSource.EMOTIV,
                    _ => PacketSource.OTHER
                };
            }
        }
        public IPAddress? SourceAddress { get; set; }
        public IPAddress? DestinationAddress { get; set; }
        public PacketSource Source { get; }
        public Byte[] Payload { get; set; } = new Byte[0];
        public int PayloadLength => Payload?.Length ?? -1;
        public DateTime Timestamp { get; set; }

        #region METHODS
        public Dictionary<int, List<(DateTime, double)>> ExtractData(int nRepetitions)
        {
            var points = new Dictionary<int, List<(DateTime, double)>>();

            return points;
        }
        #endregion
    }
}
