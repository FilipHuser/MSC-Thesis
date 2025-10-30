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
        public static Dictionary<ModuleType, Dictionary<int, List<object>>?> ProcessAll(IEnumerable<IModule> modules, Dictionary<ModuleType, int> signalCounts)
        {
            var result = new Dictionary<ModuleType, Dictionary<int, List<object>>?>();

            foreach (var module in modules)
            {
                var channelCount = signalCounts.GetValueOrDefault(module.ModuleType, 0);
                result[module.ModuleType] = channelCount > 0
                    ? ProcessModule(module, channelCount)
                    : null;
            }

            return result;
        }
        private static Dictionary<int, List<object>>? ProcessModule(IModule module, int channelCount)
        {
            if (channelCount <= 0) return null;

            var output = new Dictionary<int, List<object>>();

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
        private static void ProcessBiopacModule(BiopacSourceModule module, int channelCount, Dictionary<int, List<object>> output)
        {
            var convertFunc = ByteArrayConverter<short>.GetConvertFunction();

            foreach (var capturedData in module.Get())
            {
                var udp = ExtractUdpPacket(capturedData);
                if (udp == null) continue;

                ProcessBiopacPayload(udp.PayloadData, channelCount, convertFunc, output);
            }
        }
        private static UdpPacket? ExtractUdpPacket(CapturedData<RawCapture> capturedData)
        {
            return Packet.ParsePacket(capturedData.Data.LinkLayerType, capturedData.Data.Data)
                         .Extract<UdpPacket>();
        }
        private static void ProcessBiopacPayload(byte[] payload, int channelCount, Func<byte[], int, object> convertFunc, Dictionary<int, List<object>> output)
        {
            if (payload.Length <= 2) return;

            int samplesPerChannel = (payload.Length - 2) / sizeof(short) / channelCount;
            int totalSamples = samplesPerChannel * channelCount;

            for (int i = 0; i < totalSamples; i++)
            {
                int offset = 1 + i * sizeof(short);
                var bytes = payload.Skip(offset).Take(sizeof(short)).Reverse().ToArray();
                var value = convertFunc(bytes, 0);

                int channel = i % channelCount;
                AddToChannel(output, channel, value);
            }
        }
        private static void ProcessVRModule(VRSourceModule module, Dictionary<int, List<object>> output)
        {
            foreach (var post in module.Get())
            {
                using var doc = JsonDocument.Parse(post.Data);
                var root = doc.RootElement;

                if (root.ValueKind != JsonValueKind.Object) continue;

                int index = 0;
                foreach (var property in root.EnumerateObject())
                {
                    var value = ConvertJsonElement(property.Value);
                    if (value != null)
                    {
                        AddToChannel(output, index, value);
                    }
                    index++;
                }
            }
        }
        private static object? ConvertJsonElement(JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.String => element.GetString(),
                JsonValueKind.Number => ConvertJsonNumber(element),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Array => element.EnumerateArray()
                    .Select(ConvertJsonElement)
                    .ToList(),
                JsonValueKind.Object => element.EnumerateObject()
                    .ToDictionary(p => p.Name, p => ConvertJsonElement(p.Value)),
                JsonValueKind.Null => null,
                JsonValueKind.Undefined => null,
                _ => element.GetRawText(),
            };
        }
        private static object? ConvertJsonNumber(JsonElement element)
        {
            if (element.TryGetInt64(out var longValue))
                return longValue;

            if (element.TryGetDouble(out var doubleValue))
                return doubleValue;

            return null;
        }
        private static void AddToChannel(Dictionary<int, List<object>> output, int channel, object value)
        {
            if (!output.TryGetValue(channel, out var list))
            {
                list = new List<object>();
                output[channel] = list;
            }
            list.Add(value);
        }
    }
}