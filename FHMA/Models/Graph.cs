using LiveChartsCore.SkiaSharpView;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using System.Collections.ObjectModel;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System.Xml.Serialization;

namespace FHMA.Models
{
    public enum ModuleType
    {
        EKG,    // Electrocardiogram
        EEG,    // Electroencephalogram
        HR,     // Heart Rate
        RESP,   // Respiration
        SPO2,   // Oxygen Saturation
        EDA,    // Electrodermal Activity
        AN,     // Analog
    }

    [Serializable]
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
        public double LowerBound { get; set; }
        public double UpperBound { get; set; }
        public int PointLimit { get; set; } = 1000;

        private readonly List<DateTimePoint> _yValues = []; // _values
        private readonly DateTimeAxis _xValues; // _customAxis
        [XmlIgnore]
        public Axis[] XAxes { get; set; }
        [XmlIgnore]
        public Axis[]? YAxes { get; set; }
        [XmlIgnore]
        public ObservableCollection<ISeries> Series { get; set; }
        [XmlIgnore]
        public object Sync { get; } = new object();
        public Graph()
        {
            Series = [
                new LineSeries<DateTimePoint>
                {
                    Values = _yValues,
                    Fill = null,
                    Stroke = new SolidColorPaint(SKColors.Green) { StrokeThickness = 1 },
                    GeometryFill = null,
                    GeometryStroke = null,
                }
            ];

            _xValues = new DateTimeAxis(TimeSpan.FromSeconds(1), Formatter)
            {
                CustomSeparators = GetSeparators(),
                AnimationsSpeed = TimeSpan.FromMilliseconds(0),
                SeparatorsPaint = new SolidColorPaint(SKColors.Black.WithAlpha(100)),
            };

            XAxes = [_xValues];
        }
        public void Update(List<DateTimePoint> points)
        {
            Func<double, double> mapRange = (x) =>
            {
                return LowerBound + ((x - short.MinValue) * (UpperBound - LowerBound) / (short.MaxValue - short.MinValue));
            };

            lock (Sync)
            {
                foreach (var point in points)
                {
                    if (point.Value == null) { continue; }
                    point.Value = mapRange((double)point.Value);


                    _yValues.Add(point);
                    //if (_yValues.Count > PointLimit) { _yValues.RemoveAt(0); }
                    _xValues.CustomSeparators = GetSeparators();
                }
            }
        }
        private static double[] GetSeparators()
        {
            var now = DateTime.Now;

            return
            [
                now.AddSeconds(-25).Ticks,
                now.AddSeconds(-20).Ticks,
                now.AddSeconds(-15).Ticks,
                now.AddSeconds(-10).Ticks,
                now.AddSeconds(-5).Ticks,
                now.Ticks
            ];
        }

        private static string Formatter(DateTime date)
        {
            var secsAgo = (DateTime.Now - date).TotalSeconds;

            return secsAgo < 1
                ? "now"
                : $"{secsAgo:N0}s ago";
        }
    }
}
