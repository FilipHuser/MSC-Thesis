using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace Graphium.Controls
{
    public class YAxisControl : FrameworkElement
    {
        #region PROPERTIES
        public double MinY { get => (double)GetValue(MinYProperty); set => SetValue(MinYProperty, value); }
        public double MaxY { get => (double)GetValue(MaxYProperty); set => SetValue(MaxYProperty, value); }
        public static readonly DependencyProperty MinYProperty =
            DependencyProperty.Register(nameof(MinY), typeof(double), typeof(YAxisControl),
                new PropertyMetadata(0.0, (d, _) => ((YAxisControl)d).InvalidateVisual()));

        public static readonly DependencyProperty MaxYProperty =
            DependencyProperty.Register(nameof(MaxY), typeof(double), typeof(YAxisControl),
                new PropertyMetadata(1.0, (d, _) => ((YAxisControl)d).InvalidateVisual()));
        #endregion
        #region METHODS
        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            double range = MaxY - MinY;
            if (range <= 0 || ActualHeight <= 0) return;

            var majorPen = new Pen(Brushes.Gray, 1);
            var minorPen = new Pen(new SolidColorBrush(Color.FromRgb(200, 200, 200)), 1);
            var typeface = new Typeface("Segoe UI");
            double dpi = VisualTreeHelper.GetDpi(this).PixelsPerDip;

            dc.DrawLine(majorPen, new Point(ActualWidth, 0), new Point(ActualWidth, ActualHeight));

            double roughInterval = range / 6;
            if (roughInterval <= 1e-10) return;

            double magnitude = Math.Pow(10, Math.Floor(Math.Log10(Math.Abs(roughInterval))));
            double[] niceMultiples = { 1, 2, 2.5, 5, 10 };
            double interval = niceMultiples.First(m => m * magnitude >= roughInterval) * magnitude;
            if (interval <= 1e-10) return;

            double minorInterval = interval / 5;
            int maxTicks = 200;

            // Minor ticky
            double firstMinor = Math.Ceiling(MinY / minorInterval) * minorInterval;
            int count = 0;
            for (double tick = firstMinor; tick <= MaxY && count < maxTicks; tick += minorInterval, count++)
            {
                double y = ActualHeight - ((tick - MinY) / range) * ActualHeight;
                dc.DrawLine(minorPen, new Point(0, y), new Point(3, y));
            }

            // Major ticky + labely
            double firstMajor = Math.Ceiling(MinY / interval) * interval;
            count = 0;
            for (double tick = firstMajor; tick <= MaxY && count < maxTicks; tick += interval, count++)
            {
                double y = ActualHeight - ((tick - MinY) / range) * ActualHeight;

                dc.DrawLine(majorPen, new Point(0, y), new Point(6, y));
                dc.DrawLine(majorPen, new Point(ActualWidth - 6, y), new Point(ActualWidth, y));

                var text = new FormattedText(
                    FormatTick(tick, range),
                    CultureInfo.InvariantCulture,
                    FlowDirection.LeftToRight,
                    typeface, 10, Brushes.Gray, dpi);

                dc.DrawText(text, new Point(8, y - text.Height / 2)); // ← chybělo
            }
        }
        private static string FormatTick(double tick, double range)
        {
            if (range >= 1000) return $"{tick:F0}";
            if (range >= 10) return $"{tick:F1}";
            if (range >= 1) return $"{tick:F2}";
            return $"{tick:F3}";
        }
        #endregion
    }
}