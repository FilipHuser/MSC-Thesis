using PCAPILib;
using SharpPcap;
using PacketDotNet;
using DataHub.Modules;
using DataHub.Core;
using System.Text.Json;
using DataHub.Interfaces;

namespace Graphium.Core
{
    static class DataProcessor
    {
        public static Dictionary<int, List<object>>? Process(IModule module, int nChannels)
        {
            if (nChannels <= 0) return null;

            var output = new Dictionary<int, List<object>>();

            switch (module)
            {
                case BiopacSourceModule bsm:
                    foreach (var rc in bsm.Get())
                    {
                        var udp = Packet.ParsePacket(rc.Data.LinkLayerType, rc.Data.Data).Extract<UdpPacket>();
                        if (udp == null) continue;

                        byte[] payload = udp.PayloadData;
                        if (payload.Length <= 2) continue;

                        var convertFunc = ByteArrayConverter<short>.GetConvertFunction();
                        int nRepetitions = ((payload.Length - 2) / sizeof(short)) / nChannels;

                        for (int i = 0; i < nRepetitions * nChannels; i++)
                        {
                            int offset = 1 + i * sizeof(short);
                            var slice = payload.Skip(offset).Take(sizeof(short)).Reverse().ToArray();
                            var value = convertFunc(slice, 0);

                            if (!output.TryGetValue(i % nChannels, out var list))
                            {
                                list = new List<object>();
                                output[i % nChannels] = list;
                            }
                            list.Add(value);
                        }
                    }
                    break;

                case VRSourceModule vrsm:
                    foreach (var post in vrsm.Get())
                    {
                        using var doc = JsonDocument.Parse(post.Data);
                        var root = doc.RootElement;
                        if (root.ValueKind != JsonValueKind.Object) continue;

                        int index = 0;
                        foreach (var prop in root.EnumerateObject())
                        {
                            object? val = prop.Value.ValueKind switch
                            {
                                JsonValueKind.String => prop.Value.GetString(),
                                JsonValueKind.Number => prop.Value.TryGetInt64(out var l) ? l
                                                        : prop.Value.TryGetDouble(out var d) ? d
                                                        : null,
                                JsonValueKind.True => true,
                                JsonValueKind.False => false,
                                JsonValueKind.Null => null,
                                _ => prop.Value.GetRawText()
                            };

                            if (val != null)
                            {
                                if (!output.TryGetValue(index, out var list))
                                {
                                    list = new List<object>();
                                    output[index] = list;
                                }
                                list.Add(val);
                            }
                            index++;
                        }
                    }
                    break;
            }

            return output;
        }

    }
}
