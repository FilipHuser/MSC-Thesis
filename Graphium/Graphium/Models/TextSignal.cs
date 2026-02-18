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

        public TextSignal(string name, ModuleType source)
            : base(name, SignalType.Text, source) { }

        public override void Update(double absoluteTimeSeconds, object? data)
        {
            if (data == null) return;
            YData.Add(data.ToString() ?? string.Empty);
            XData.Add(absoluteTimeSeconds);
        }

        public override void ClearData()
        {
            base.ClearData();
            YData.Clear();
        }
    }
}
