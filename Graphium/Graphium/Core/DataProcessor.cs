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
        #region METHODS
        private static string NormalizeKey(string key) => Regex.Replace(key, @"\s+", string.Empty);

        private static object? ConvertElement(JsonElement element) => element.ValueKind switch
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
        public static Dictionary<ModuleType, List<List<Sample>>> ProcessAll(IEnumerable<IModule> modules, IEnumerable<SignalBase>? signals)
        {
            ArgumentNullException.ThrowIfNull(signals);

            var result = new Dictionary<ModuleType, List<List<Sample>>>();
            var signalsByModule = signals.GroupBy(s => s.Source)
                                         .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var module in modules)
            {
                result[module.ModuleType] = signalsByModule.TryGetValue(module.ModuleType, out var moduleSignals) && moduleSignals.Any()
                    ? ProcessModule(module, moduleSignals) : [];
            }

            return result;
        }
        private static List<List<Sample>> ProcessModule(IModule module, List<SignalBase> moduleSignals)
        {
            if (moduleSignals.Count <= 0) return [];

            return module switch
            {
                PcapSourceModule pcap => ProcessPcapModule(pcap, moduleSignals.Count, moduleSignals),
                HttpSourceModule http => ProcessJsonModule(http, moduleSignals, c => c.Data),
                UdpSourceModule udp => ProcessJsonModule(udp, moduleSignals, c => System.Text.Encoding.UTF8.GetString(c.Data)),
                _ => []
            };
        }
        private static List<List<Sample>> ProcessJsonModule<T>(ModuleBase<T> module, List<SignalBase> moduleSignals, Func<CapturedData<T>, string> getJson)
        {
            var output = new List<List<Sample>>();
            var signalMap = moduleSignals.ToDictionary(
                s => NormalizeKey(s.Name), s => s, StringComparer.OrdinalIgnoreCase);
            var samples = new List<Sample>();

            foreach (var captured in module.Get())
            {
                string json;
                try { json = getJson(captured); }
                catch { continue; }

                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                if (root.ValueKind != JsonValueKind.Object) continue;

                var receivedData = root.EnumerateObject()
                    .ToDictionary(p => NormalizeKey(p.Name), p => ConvertElement(p.Value), StringComparer.OrdinalIgnoreCase);

                var sample = new Sample(captured.Timestamp);
                foreach (var (key, signal) in signalMap)
                {
                    if (receivedData.TryGetValue(key, out var val) && val != null)
                        sample.Channels[signal] = val;
                }

                samples.Add(sample);
            }

            if (samples.Count > 0)
                output.Add(samples);

            return output;
        }
        private static List<List<Sample>> ProcessPcapModule(PcapSourceModule module, int channelCount, List<SignalBase> moduleSignals)
        {
            var output = new List<List<Sample>>();
            var convertFunc = ByteArrayConverter<short>.GetConvertFunction();

            foreach (var capturedData in module.Get())
            {
                var udp = Packet.ParsePacket(capturedData.Data.LinkLayerType, capturedData.Data.Data).Extract<UdpPacket>();
                if (udp == null) continue;

                var payload = udp.PayloadData;
                if (payload.Length < 2 + (2 * channelCount)) continue;

                int dataBytes = payload.Length - 2;
                if (dataBytes % (sizeof(short) * channelCount) != 0) continue;

                int samplesPerChannel = dataBytes / sizeof(short) / channelCount;
                var packetSamples = new List<Sample>();

                for (int sampleIndex = 0; sampleIndex < samplesPerChannel; sampleIndex++)
                {
                    var sample = new Sample(capturedData.Timestamp);
                    for (int channel = 0; channel < channelCount; channel++)
                    {
                        int offset = 1 + (sampleIndex * channelCount + channel) * sizeof(short);
                        var bytes = new byte[] { payload[offset + 1], payload[offset] };
                        sample.Channels[moduleSignals[channel]] = convertFunc(bytes, 0);
                    }
                    packetSamples.Add(sample);
                }

                output.Add(packetSamples);
            }

            return output;
        }
        #endregion
    }
}