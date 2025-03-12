using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using System.Collections.ObjectModel;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System.Runtime.Serialization;
using FHAPILib;
using FHAPI.Core;
using System.ComponentModel;

namespace FHMA.Models
{
    public enum ModuleType
    {
        EKG,    // Electrocardiogram
        EEG,    // Electroencephalogram
        HR,     // Heart Rate
        RESP,   // Respiration
        SPO2,   // Oxygen Saturation
        EDA     // Electrodermal Activity
    }
    public class Graph
    {
        private int _channel;
        public int Channel
        {
            get => _channel;
            set
            {
                if (value < 0) { throw new ArgumentOutOfRangeException(nameof(value), "Channel number cannot be negative!"); }
                _channel = value;
            }
        }
        public ModuleType ModuleType { get; set; }

        public bool IsReading { get; set; } = true;
        private readonly Random _random = new();
        public Axis[] XAxes { get; set; }
        private readonly List<DateTimePoint> _values = [];
        private readonly DateTimeAxis _customAxis;
        public ObservableCollection<ISeries> Series { get; set; }
        public object Sync { get; } = new object();
        public Graph()
        {
            Series = [
                new LineSeries<DateTimePoint>
                {
                    Values = _values,
                    Fill = null,
                    GeometryFill = null,
                    GeometryStroke = null
                }
            ];

            _customAxis = new DateTimeAxis(TimeSpan.FromSeconds(1), Formatter)
            {
                AnimationsSpeed = TimeSpan.FromMilliseconds(0),
                SeparatorsPaint = new SolidColorPaint(SKColors.Black.WithAlpha(100))
            };

            XAxes = [_customAxis];
        }

        private static double[] GetSeparators()
        {
            var now = DateTime.Now;

            return
            [
                now.Ticks
            ];
        }


        public void Update(List<FHPacket> values)
        {
            var firstPacket = values.FirstOrDefault()?.Payload.ToArray();
            var asdf = values.FirstOrDefault()?.Payload.Skip(Channel * 2).Take(2).ToArray();

            var dateTimePoints = values.Select(x => new DateTimePoint(DateTime.Now, Convertor<short>.ConvertPayload(x.Payload.Skip(Channel*2).Take(2).ToArray(), 0))).ToList();
            if (dateTimePoints.Count == 0) { return; }

            lock (Sync)
            {
                _values.Add(dateTimePoints.First());
                if (_values.Count > 100) { _values.RemoveAt(0); }
                _customAxis.CustomSeparators = GetSeparators();
            }
        }
        private static string Formatter(DateTime date)
        {
            return $"{date:HH:mm:ss}";
        }
    }
}
