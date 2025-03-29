using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Channels;
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

                var convertFunc =  Converter<sbyte>.GetPayloadConvertFunction();

                Source = convertFunc(Payload.Take(1).ToArray() , 0) switch
                {
                    1 => PacketSource.BIOPAC,
                    2 => PacketSource.EMOTIV,
                    _ => null
                };
            }
        }
        public IPAddress? SourceAddress { get; set; }
        public IPAddress? DestinationAddress { get; set; }
        public PacketSource? Source { get; }
        public Byte[] Payload { get; set; } = new Byte[0];
        public int PayloadLength => Payload?.Length ?? -1;
        public DateTime Timestamp { get; set; }

        public Dictionary<int, List<(DateTime, double)>>? ExtractData(int nChannels)
        {
            var data = new Dictionary<int, List<(DateTime, double)>>();

            var (convertFunc, payloadElementSize) = Source switch
            {
                PacketSource.BIOPAC => (Converter<short>.GetPayloadConvertFunction(), sizeof(short)),
                PacketSource.EMOTIV => (Converter<int>.GetPayloadConvertFunction(), sizeof(int)),
                _ => (null, 0)
            };

            if (payloadElementSize == 0 || convertFunc == null) { return null; }

            int useablePayload = PayloadLength - 2;
            int nRepetitions = (useablePayload / payloadElementSize) / nChannels;

            for (int i = 0; i < nRepetitions * nChannels; i++)
            {
                int offset = 1 + (i * payloadElementSize);

                var payloadSlice = Payload.Skip(offset).Take(payloadElementSize).ToArray();

                if (Source == PacketSource.BIOPAC) { Array.Reverse(payloadSlice); }

                var value = convertFunc(payloadSlice, 0);

                if (!data.TryGetValue(i % nChannels, out var existingList))
                {
                    existingList = new List<(DateTime, double)>();
                    data[i % nChannels] = existingList;
                }

                existingList.Add((Timestamp, value));
            }

            return data;
        }
    }
}
