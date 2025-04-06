using System.Net;
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
            STATION,
        }
        public IPAddress? SourceAddress { get; set; }
        public IPAddress? DestinationAddress { get; set; }
        public PacketSource? Source { get; }
        public Byte[] Payload { get; set; } = new byte[0];
        public int PayloadLength => Payload?.Length ?? -1;
        public DateTime Timestamp { get; set; }

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

                Source = (sbyte)convertFunc(Payload.Take(1).ToArray(), 0) switch
                {
                    1 => PacketSource.BIOPAC,      // 1
                    2 => PacketSource.EMOTIV,      // 2
                    123 => PacketSource.STATION,    // {
                    _ => null
                };
            }
        }
        #region METHODS
        public Dictionary<int, List<(DateTime, object)>>? ExtractData(int nChannels)
        {
            var data = new Dictionary<int, List<(DateTime, object)>>();

            var (convertFunc, payloadElementSize) = Source switch
            {
                PacketSource.BIOPAC => (Converter<short>.GetPayloadConvertFunction(), sizeof(short)),
                PacketSource.EMOTIV => (Converter<int>.GetPayloadConvertFunction(), sizeof(int)),
                PacketSource.STATION => (Converter<sbyte>.GetPayloadConvertFunction(), sizeof(sbyte)),
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
                    existingList = new List<(DateTime, object)>();
                    data[i % nChannels] = existingList;
                }

                existingList.Add((Timestamp, value));
            }
            return data;
        }
        #endregion
    }
}
