using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace Graphium.Controls
{
    public class XAxisControl : FrameworkElement
    {
        #region PROPERTIES
        public double MinX { get => (double)GetValue(MinXProperty); set => SetValue(MinXProperty, value); }
        public double MaxX { get => (double)GetValue(MaxXProperty); set => SetValue(MaxXProperty, value); }
        public double DataAreaLeft { get => (double)GetValue(DataAreaLeftProperty); set => SetValue(DataAreaLeftProperty, value); }
        public double DataAreaRight { get => (double)GetValue(DataAreaRightProperty); set => SetValue(DataAreaRightProperty, value); }

        public static readonly DependencyProperty MinXProperty =
            DependencyProperty.Register(nameof(MinX), typeof(double), typeof(XAxisControl),
                new PropertyMetadata(0.0, (d, _) => ((XAxisControl)d).InvalidateVisual()));

        public static readonly DependencyProperty MaxXProperty =
            DependencyProperty.Register(nameof(MaxX), typeof(double), typeof(XAxisControl),
                new PropertyMetadata(1.0, (d, _) => ((XAxisControl)d).InvalidateVisual()));
        public static readonly DependencyProperty DataAreaLeftProperty =
            DependencyProperty.Register(nameof(DataAreaLeft), typeof(double), typeof(XAxisControl),
                new PropertyMetadata(0.0, (d, _) => ((XAxisControl)d).InvalidateVisual()));

        public static readonly DependencyProperty DataAreaRightProperty =
            DependencyProperty.Register(nameof(DataAreaRight), typeof(double), typeof(XAxisControl),
                new PropertyMetadata(0.0, (d, _) => ((XAxisControl)d).InvalidateVisual()));
        #endregion
        #region METHODS
        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);
            System.Diagnostics.Debug.WriteLine($"DataAreaLeft: {DataAreaLeft}, DataAreaRight: {DataAreaRight}, ActualWidth: {ActualWidth}");
            double left = DataAreaLeft > 0 ? DataAreaLeft : 0;
            double right = DataAreaRight > 0 ? ActualWidth - DataAreaRight : ActualWidth;
            double plotWidth = right - left;
            double range = MaxX - MinX;
            if (range <= 0 || plotWidth <= 0) return;

            var majorPen = new Pen(Brushes.Gray, 1);
            var minorPen = new Pen(new SolidColorBrush(Color.FromRgb(200, 200, 200)), 1);
            var typeface = new Typeface("Segoe UI");
            double dpi = VisualTreeHelper.GetDpi(this).PixelsPerDip;

            dc.DrawLine(majorPen, new Point(left, 1), new Point(right, 1));

            double roughInterval = range / 8;
            if (roughInterval <= 1e-10) return;

            double magnitude = Math.Pow(10, Math.Floor(Math.Log10(Math.Abs(roughInterval))));
            double[] niceMultiples = { 1, 2, 2.5, 5, 10 };
            double interval = niceMultiples.First(m => m * magnitude >= roughInterval) * magnitude;
            if (interval <= 1e-10) return;

            double minorInterval = interval / 5;
            if (minorInterval <= 1e-10) return;

            int maxTicks = 500;

            double firstMinor = Math.Ceiling(MinX / minorInterval) * minorInterval;
            int count = 0;
            for (double tick = firstMinor; tick <= MaxX && count < maxTicks; tick += minorInterval, count++)
            {
                double x = left + ((tick - MinX) / range) * plotWidth;
                dc.DrawLine(minorPen, new Point(x, 1), new Point(x, 4));
            }

            double firstMajor = Math.Ceiling(MinX / interval) * interval;
            count = 0;
            for (double tick = firstMajor; tick <= MaxX && count < maxTicks; tick += interval, count++)
            {
                double x = left + ((tick - MinX) / range) * plotWidth;
                dc.DrawLine(majorPen, new Point(x, 1), new Point(x, 7));

                var text = new FormattedText(
                    FormatTick(tick, range),
                    CultureInfo.InvariantCulture,
                    FlowDirection.LeftToRight,
                    typeface, 10, Brushes.Gray, dpi);

                dc.DrawText(text, new Point(x - text.Width / 2, 9));
            }
        }
        private static string FormatTick(double tick, double range)
        {
            if (range >= 60000) return $"{tick / 60000:F1}min";
            if (range >= 10000) return $"{tick / 1000:F1}s";
            if (range >= 1000) return $"{tick:F0}ms";
            if (range >= 100) return $"{tick:F1}ms";
            if (range >= 10) return $"{tick:F2}ms";
            if (range >= 1) return $"{tick:F3}ms";
            return $"{tick:F4}ms";
        }
        #endregion
    }
}