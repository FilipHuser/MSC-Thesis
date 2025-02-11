using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using PacketDotNet;
using SharpPcap;

namespace FHAPI.Core
{
    public class FHPacket
    {
        public FHPacket(RawCapture packet)
        {
            var parsedPacket = Packet.ParsePacket(packet.LinkLayerType, packet.Data);
            if (parsedPacket is EthernetPacket ethPacket && ethPacket.PayloadPacket is IPv4Packet ipPacket &&
                ipPacket.PayloadPacket is UdpPacket udpPacket)
            {
                SourceAddress = ipPacket.SourceAddress;
                DestinationAddress = ipPacket.DestinationAddress;
                Payload = udpPacket.PayloadData;
            }
        }
        public IPAddress? SourceAddress { get; set; }
        public IPAddress? DestinationAddress { get; set; }
        public Byte[] Payload { get; set; } = new Byte[0];
        public int PayloadLenght => Payload?.Length ?? -1;
    }
}
