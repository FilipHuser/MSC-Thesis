﻿using System;
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
        public double LowerBound { get; set; } = -10;
        public double UpperBound { get; set; } = 10;
        public int PointLimit { get; set; } = 2000;

        private readonly List<DateTimePoint> _yValues = [];
        private readonly DateTimeAxis _xValues;
        public Axis[] XAxes { get; set; }
        public ObservableCollection<ISeries> Series { get; set; }
        public object Sync { get; } = new object();
        public Graph()
        {
            Series = [
                new LineSeries<DateTimePoint>
                {
                    Values = _yValues,
                    Fill = null,
                    Stroke = new SolidColorPaint(SKColors.Blue) { StrokeThickness = 1 },
                    GeometryFill = null,
                    GeometryStroke = null,
                    LineSmoothness = 1
                }
            ];

            _xValues = new DateTimeAxis(TimeSpan.FromSeconds(1), Formatter)
            {
                AnimationsSpeed = TimeSpan.FromMilliseconds(0),
                SeparatorsPaint = new SolidColorPaint(SKColors.Black.WithAlpha(50))
            };

            XAxes = [_xValues];
        }
        private static double[] GetSeparators()
        {
            var now = DateTime.Now;

            return
            [
                now.Ticks
            ];
        }
        public void Update(List<FHPacket> packets)
        {
            var convertType = typeof(short);

            int offset =  1 + (Channel * 2);
            //int skip = 

            Func<short , double> mapRange = (x) => {
                return LowerBound + (((x - short.MinValue) * (UpperBound - LowerBound) / (short.MaxValue - short.MinValue)));
            };

            var dateTimePoints = new List<DateTimePoint>();

            foreach (var packet in packets)
            {
                // First value: Take 2 bytes after skipping 'offset'
                var firstValue = Convertor<short>.ConvertPayload(packet.Payload.Skip(offset).Take(2).ToArray(), 0) ?? 0;

                // Second value: Take another 2 bytes after skipping the offset for the second value
                var secondValue = Convertor<short>.ConvertPayload(packet.Payload.Skip(offset + 4).Take(2).ToArray(), 0) ?? 0;

                // Add both values to DateTimePoints list (you may want to map both values to DateTimePoint)
                dateTimePoints.Add(new DateTimePoint(packet.Timestamp, mapRange(firstValue)));
                dateTimePoints.Add(new DateTimePoint(packet.Timestamp, mapRange(secondValue)));
            }

            if (dateTimePoints.Count == 0) { return; }

            lock (Sync)
            {
                foreach (var point in dateTimePoints)
                {
                    _yValues.Add(point);
                    if (_yValues.Count > PointLimit) { _yValues.RemoveAt(0); }
                    _xValues.CustomSeparators = GetSeparators();
                }
            }
        }
        private static string Formatter(DateTime date)
        {
            return $"{date:HH:mm:ss}";
        }
    }
}
