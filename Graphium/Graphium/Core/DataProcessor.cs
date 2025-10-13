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
        private static object? JsonElementToObject(JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.String => element.GetString(),

                JsonValueKind.Number => element.TryGetInt64(out var l) ? l
                                        : element.TryGetDouble(out var d) ? d
                                        : null,

                JsonValueKind.True => true,
                JsonValueKind.False => false,

                JsonValueKind.Array => element.EnumerateArray()
                                              .Select(JsonElementToObject)
                                              .ToList(),

                JsonValueKind.Object => element.EnumerateObject()
                    .ToDictionary(p => p.Name, p => JsonElementToObject(p.Value)),

                JsonValueKind.Null => null,
                JsonValueKind.Undefined => null,

                _ => element.GetRawText(),
            };
        }
        public static Dictionary<ModuleType, Dictionary<int, List<object>>?>? ProcessAll(IEnumerable<IModule> modules, Dictionary<ModuleType, int> signalCounts)
        {
            var result = new Dictionary<ModuleType, Dictionary<int, List<object>>?>();

            foreach (var module in modules)
            {
                if (!signalCounts.TryGetValue(module.ModuleType, out int channelCount) || channelCount <= 0)
                {
                    result[module.ModuleType] = null;
                    continue;
                }

                var data = ProcessModule(module, channelCount);
                result[module.ModuleType] = data;
            }

            return result;
        }

        private static Dictionary<int, List<object>>? ProcessModule(IModule module, int nChannels)
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

                            int channel = i % nChannels;
                            if (!output.TryGetValue(channel, out var list))
                            {
                                list = new List<object>();
                                output[channel] = list;
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
                                JsonValueKind.Array => prop.Value.EnumerateArray().Select(JsonElementToObject).ToList(),
                                JsonValueKind.Null => null,
                                _ => prop.Value.GetRawText(),
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

            return output.Count > 0 ? output : null;
        }
    }
}
