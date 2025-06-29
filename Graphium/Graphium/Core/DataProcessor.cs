using PCAPILib;
using SharpPcap;
using PacketDotNet;
using DataHub.Modules;
using DataHub.Core;
using System.Text.Json;

namespace Graphium.Core
{
    static class DataProcessor
    {
        public static Dictionary<int , List<object>> Process(ModuleBase moduleBase , int nChannels)
        {
            var output = new Dictionary<int , List<object>>();

            switch(moduleBase)
            {
                case PacketModule packetModule:

                        var packets = packetModule.Get<RawCapture>(x => true).Select(x => x.Data).ToList();
                        if (packets.Count == 0) { return output; }

                    var convertFunc = ByteArrayConverter<short>.GetConvertFunction();

                    foreach(var rc in packets)
                    {
                        var udpPacket = Packet.ParsePacket(rc.LinkLayerType, rc.Data).Extract<UdpPacket>();
                        if (udpPacket == null) { continue; }

                        byte[] payload = udpPacket.PayloadData;
                        int payloadLength = payload.Length;
                        int payloadElementSize = sizeof(short);
                        int useablePayload = payloadLength - 2;
                        int nRepetitions = (useablePayload / payloadElementSize) / nChannels;

                        for (int i = 0; i < nRepetitions * nChannels; i++)
                        {
                            int offset = 1 + (i * payloadElementSize);

                            var payloadSlice = payload.Skip(offset).Take(payloadElementSize).ToArray();

                            Array.Reverse(payloadSlice);

                            var value = convertFunc(payloadSlice, 0);

                            if (!output.TryGetValue(i % nChannels, out var existingList))
                            {
                                existingList = new List<object>();
                                output[i % nChannels] = existingList;
                            }
                            existingList.Add(value);
                        }
                    }    
                    break;

                case HTTPModule<string> httpModule:

                    var postRequests = httpModule.Get<string>(x => true).Select(x => x.Data).ToList();
                    if (postRequests.Count() == 0) { return output; }

                    var jsonOpts = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    foreach (var pr in postRequests)
                    {
                        using var jsonDoc = JsonDocument.Parse(pr);
                        var root = jsonDoc.RootElement;

                        if (root.ValueKind != JsonValueKind.Object)
                            continue;

                        int index = 0;
                        foreach (var property in root.EnumerateObject())
                        {
                            object? val = property.Value.ValueKind switch
                            {
                                JsonValueKind.String => property.Value.GetString(),
                                JsonValueKind.Number => property.Value.TryGetInt64(out var l) ? l
                                                        : property.Value.TryGetDouble(out var d) ? d
                                                        : (object?)null,
                                JsonValueKind.True => true,
                                JsonValueKind.False => false,
                                JsonValueKind.Null => null,
                                _ => property.Value.GetRawText()  // fallback for complex types
                            };

                            if (!output.TryGetValue(index, out var list))
                            {
                                list = new List<object>();
                                output[index] = list;
                            }

                            if(val != null) { list.Add(val); }
                            index++;
                        }
                    }

                    break;
            }    

            return output;
        }
    }
}
