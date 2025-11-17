using PacketDotNet;
using DataHub.Modules;
using DataHub.Core;
using System.Text.Json;
using DataHub.Interfaces;
using SharpPcap;

namespace Graphium.Core
{
    static class DataProcessor
    {
        public static Dictionary<ModuleType, Dictionary<int, List<(object value, DateTime timestamp)>>?> ProcessAll(
            IEnumerable<IModule> modules,
            Dictionary<ModuleType, int> signalCounts)
        {
            var result = new Dictionary<ModuleType, Dictionary<int, List<(object value, DateTime timestamp)>>?>();

            foreach (var module in modules)
            {
                var channelCount = signalCounts.GetValueOrDefault(module.ModuleType, 0);
                result[module.ModuleType] = channelCount > 0 ? ProcessModule(module, channelCount) : null;
            }

            return result;
        }

        private static Dictionary<int, List<(object value, DateTime timestamp)>>? ProcessModule(IModule module, int channelCount)
        {
            if (channelCount <= 0) return null;

            var output = new Dictionary<int, List<(object value, DateTime timestamp)>>();

            switch (module)
            {
                case BiopacSourceModule biopac:
                    ProcessBiopacModule(biopac, channelCount, output);
                    break;
                case VRSourceModule vr:
                    ProcessVRModule(vr, output);
                    break;
            }

            return output.Count > 0 ? output : null;
        }

        private static void ProcessBiopacModule(BiopacSourceModule module, int channelCount,
            Dictionary<int, List<(object value, DateTime timestamp)>> output)
        {
            var convertFunc = ByteArrayConverter<short>.GetConvertFunction();

            foreach (var capturedData in module.Get())
            {
                var udp = Packet.ParsePacket(capturedData.Data.LinkLayerType, capturedData.Data.Data).Extract<UdpPacket>();
                if (udp == null) continue;

                var timestamp = capturedData.Timestamp;
                var payload = udp.PayloadData;

                if (payload.Length <= 2) return;

                int samplesPerChannel = (payload.Length - 2) / sizeof(short) / channelCount;
                int totalSamples = samplesPerChannel * channelCount;

                for (int i = 0; i < totalSamples; i++)
                {
                    int offset = 1 + i * sizeof(short);
                    var bytes = payload.Skip(offset).Take(sizeof(short)).Reverse().ToArray();
                    var value = convertFunc(bytes, 0);
                    int channel = i % channelCount;

                    AddSample(output, channel, value, timestamp);
                }
            }
        }
        private static void ProcessVRModule(VRSourceModule module, Dictionary<int, List<(object value, DateTime timestamp)>> output)
        {
            object? ConvertElement(JsonElement element) => element.ValueKind switch
            {
                JsonValueKind.String => element.GetString(),
                JsonValueKind.Number => element.TryGetInt64(out var l) ? l :
                                        element.TryGetDouble(out var d) ? d : null,
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Array => element.EnumerateArray().Select(ConvertElement).ToList(),
                JsonValueKind.Object => element.EnumerateObject().ToDictionary(p => p.Name, p => ConvertElement(p.Value)),
                _ => null
            };

            foreach (var post in module.Get())
            {
                var timestamp = DateTime.Now;
                using var doc = JsonDocument.Parse(post.Data);
                var root = doc.RootElement;
                if (root.ValueKind != JsonValueKind.Object) continue;

                int index = 0;
                foreach (var property in root.EnumerateObject())
                {
                    var value = ConvertElement(property.Value);
                    if (value != null)
                    {
                        AddSample(output, index, value, timestamp);
                    }
                    index++;
                }
            }
        }
        private static void AddSample(Dictionary<int, List<(object value, DateTime timestamp)>> output, int channel, object value, DateTime timestamp)
        {
            if (!output.ContainsKey(channel))
            {
                output[channel] = new List<(object value, DateTime timestamp)>();
            }
            output[channel].Add((value, timestamp));
        }
    }
}