using PacketDotNet;
using DataHub.Modules;
using DataHub.Core;
using System.Text.Json;
using DataHub.Interfaces;
using SharpPcap;
using Graphium.Models;
using System.Text.RegularExpressions;

namespace Graphium.Core
{
    static class DataProcessor
    {
        private static string NormalizeKey(string key)
        {
            return Regex.Replace(key, @"\s+", string.Empty);
        }

        public static Dictionary<ModuleType, Dictionary<int, List<(object value, DateTime timestamp)>>?> ProcessAll(
            IEnumerable<IModule> modules,
            IEnumerable<Signal> signals)
        {
            var result = new Dictionary<ModuleType, Dictionary<int, List<(object value, DateTime timestamp)>>?>();
            var signalsByModule = signals.GroupBy(s => s.Source)
                                         .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var module in modules)
            {
                if (signalsByModule.TryGetValue(module.ModuleType, out var moduleSignals) && moduleSignals.Any())
                {
                    result[module.ModuleType] = ProcessModule(module, moduleSignals);
                }
                else
                {
                    result[module.ModuleType] = null;
                }
            }

            return result;
        }
        private static Dictionary<int, List<(object value, DateTime timestamp)>>? ProcessModule(IModule module, List<Signal> moduleSignals)
        {
            var channelCount = moduleSignals.Count;
            if (channelCount <= 0) return null;

            var output = new Dictionary<int, List<(object value, DateTime timestamp)>>();

            switch (module)
            {
                case BiopacSourceModule biopac:
                    ProcessBiopacModule(biopac, channelCount, output);
                    break;
                case VRSourceModule vr:
                    ProcessVRModule(vr, output, moduleSignals);
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
        private static void ProcessVRModule(VRSourceModule module, Dictionary<int, List<(object value, DateTime timestamp)>> output, List<Signal> moduleSignals)
        {
            object? ConvertElement(JsonElement element) => element.ValueKind switch
            {
                JsonValueKind.String => element.GetString(),
                JsonValueKind.Number => element.TryGetInt64(out var l) ? (object)l :
                                         element.TryGetDouble(out var d) ? (object)d : null,
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Array => element.EnumerateArray().Select(ConvertElement).ToList(),
                JsonValueKind.Object => element.EnumerateObject().ToDictionary(p => p.Name, p => ConvertElement(p.Value)),
                _ => null
            };
            var signalNameToChannelIndex = moduleSignals
                .Select((signal, index) => new { NormalizedName = NormalizeKey(signal.Name), index })
                .ToDictionary(x => x.NormalizedName, x => x.index, StringComparer.OrdinalIgnoreCase);

            foreach (var post in module.Get())
            {
                var timestamp = DateTime.Now;
                using var doc = JsonDocument.Parse(post.Data);
                var root = doc.RootElement;
                if (root.ValueKind != JsonValueKind.Object) continue;

                var receivedData = root.EnumerateObject()
                    .ToDictionary(p => NormalizeKey(p.Name), p => ConvertElement(p.Value), StringComparer.OrdinalIgnoreCase);

                foreach (var mapping in signalNameToChannelIndex)
                {
                    var normalizedSignalName = mapping.Key;
                    var channelIndex = mapping.Value;

                    if (receivedData.TryGetValue(normalizedSignalName, out object? value))
                    {
                        AddSample(output, channelIndex, value!, timestamp);
                    }
                    else
                    {
                        AddSample(output, channelIndex, null!, timestamp);
                    }
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