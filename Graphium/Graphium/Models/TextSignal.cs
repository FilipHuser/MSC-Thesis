using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using DataHub.Core;
using Graphium.Enums;

namespace Graphium.Models
{
    public class TextSignal : SignalBase
    {
        [JsonIgnore] public List<string> YData { get; set; } = new();
        public TextSignal() { }
        public TextSignal(string name, ModuleType source) : base(name, SignalType.Text, source) { }
        public override void Update(double absoluteTimeSeconds, object? data)
        {
            if (data == null) return;

            string value;
            try
            {
                var json = System.Text.Json.JsonSerializer.Serialize(data, data.GetType());
                using var doc = System.Text.Json.JsonDocument.Parse(json);
                var root = doc.RootElement;

                value = root.ValueKind switch
                {
                    System.Text.Json.JsonValueKind.Array =>
                        $"[{root.GetArrayLength()} items] {root.EnumerateArray().FirstOrDefault()}",
                    System.Text.Json.JsonValueKind.Object =>
                        string.Join(" | ", root.EnumerateObject().Take(3).Select(p => $"{p.Name}: {p.Value}")),
                    _ => json
                };
            }
            catch
            {
                value = data.ToString() ?? string.Empty;
            }

            YData.Add(value);
            XData.Add(absoluteTimeSeconds);
        }
        public override void ClearData()
        {
            base.ClearData();
            YData.Clear();
        }
    }
}
