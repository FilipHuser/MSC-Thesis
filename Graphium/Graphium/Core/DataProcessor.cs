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
        private static string NormalizeKey(string key) => Regex.Replace(key, @"\s+", string.Empty);
        public static Dictionary<ModuleType, List<List<Sample>>> ProcessAll(IEnumerable<IModule> modules, IEnumerable<Signal> signals)
        {
            var result = new Dictionary<ModuleType, List<List<Sample>>>();
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
        private static List<List<Sample>> ProcessModule(IModule module, List<Signal> moduleSignals)
        {
            var channelCount = moduleSignals.Count;
            if (channelCount <= 0) return new List<List<Sample>>();

            switch (module)
            {
                case BiopacSourceModule biopac:
                    return ProcessBiopacModule(biopac, channelCount, moduleSignals);
                case VRSourceModule vr:
                    return ProcessVRModule(vr, moduleSignals);
                default:
                    return new List<List<Sample>>();
            }
        }
        private static List<List<Sample>> ProcessBiopacModule(BiopacSourceModule module, int channelCount, List<Signal> moduleSignals)
        {
            var output = new List<List<Sample>>();
            var convertFunc = ByteArrayConverter<short>.GetConvertFunction();

            //ITERATING PACKETS
            foreach (var capturedData in module.Get())
            {
                var packetSamples = new List<Sample>();

                var udp = Packet.ParsePacket(capturedData.Data.LinkLayerType, capturedData.Data.Data).Extract<UdpPacket>();
                if (udp == null) continue;

                var timestamp = capturedData.Timestamp;
                var payload = udp.PayloadData;

                // Minimum: header(1) + one sample per channel(2*channelCount) + checksum(1)
                if (payload.Length < 2 + (2 * channelCount))
                { 
                    continue; 
                }

                int dataBytes = payload.Length - 2;

                // Validate packet has complete samples
                if (dataBytes % (sizeof(short) * channelCount) != 0) { continue; }

                int samplesPerChannel = dataBytes / sizeof(short) / channelCount;

                // 01 xx yy xx yy cs
                for (int sampleIndex = 0; sampleIndex < samplesPerChannel; sampleIndex++)
                {
                    var sample = new Sample(timestamp);

                    // Extract all channel values for this sample
                    for (int channel = 0; channel < channelCount; channel++)
                    {
                        int pairIndex = sampleIndex * channelCount + channel;
                        int offset = 1 + pairIndex * sizeof(short);
                        var bytes = new byte[] { payload[offset + 1], payload[offset] };
                        var value = convertFunc(bytes, 0);

                        sample.Channels[moduleSignals[channel]] = value;
                    }

                    packetSamples.Add(sample);
                }
                output.Add(packetSamples);
            }

            return output;
        }
        private static List<List<Sample>> ProcessVRModule(VRSourceModule module, List<Signal> moduleSignals)
        {
            var output = new List<List<Sample>>();

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

            var signalNameToSignal = moduleSignals
                .ToDictionary(signal => NormalizeKey(signal.Name), signal => signal, StringComparer.OrdinalIgnoreCase);

            var samples = new List<Sample>();

            foreach (var post in module.Get())
            {
                var sample = new Sample(post.Timestamp);

                using var doc = JsonDocument.Parse(post.Data);
                var root = doc.RootElement;
                if (root.ValueKind != JsonValueKind.Object) continue;

                var receivedData = root.EnumerateObject()
                    .ToDictionary(p => NormalizeKey(p.Name), p => ConvertElement(p.Value), StringComparer.OrdinalIgnoreCase);

                foreach (var signalMapping in signalNameToSignal)
                {
                    var normalizedSignalName = signalMapping.Key;
                    var signal = signalMapping.Value;

                    if (receivedData.TryGetValue(normalizedSignalName, out object? value))
                    {
                        sample.Channels[signal] = value;
                    }
                    else
                    {
                        sample.Channels[signal] = null;
                    }
                }

                samples.Add(sample);
            }

            if (samples.Count > 0)
            {
                output.Add(samples);
            }

            return output;
        }
    }
}