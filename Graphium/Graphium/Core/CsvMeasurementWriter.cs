using System.Globalization;
using System.IO;
using System.Text;
using Graphium.Models;

namespace Graphium.Core
{
    public class CsvMeasurementWriter : IDisposable
    {
        internal const string COLUMN_DELIMITER = ";";
        internal const string LIST_DELIMITER = ",";
        internal const int BUFFER_SIZE = 65536;

        private readonly StreamWriter _writer;
        private readonly List<Signal> _signals;
        private bool _disposed;

        internal CsvMeasurementWriter(string filePath, List<Signal> signals)
        {
            _signals = signals.OrderBy(x => x.Source).ToList();

            // Create directory if it doesn't exist
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            _writer = new StreamWriter(filePath, false, Encoding.UTF8, BUFFER_SIZE);

            // Write header
            WriteHeader();
        }

        private void WriteHeader()
        {
            var headers = new List<string> { "Timestamp" };

            foreach (var signal in _signals)
            {
                headers.Add(signal.Name);
            }

            _writer.WriteLine(string.Join(COLUMN_DELIMITER, headers));
        }

        public void WriteRow(double timestamp, Dictionary<Signal, object> values)
        {
            var row = new List<string>
            {
                timestamp.ToString("F3", CultureInfo.InvariantCulture)
            };

            foreach (var signal in _signals)
            {
                if (values.TryGetValue(signal, out var value))
                {
                    row.Add(FormatValue(value));
                }
                else
                {
                    row.Add(string.Empty);
                }
            }

            _writer.WriteLine(string.Join(COLUMN_DELIMITER, row));
        }

        public async Task WriteRowAsync(DateTime timestamp, Dictionary<Signal, object> values)
        {
            long unixTimestampUs = ((DateTimeOffset)timestamp).ToUnixTimeMilliseconds() * 1000
                          + (timestamp.Ticks % TimeSpan.TicksPerMillisecond) / 10;

            var row = new List<string>
            {
                unixTimestampUs.ToString(CultureInfo.InvariantCulture)
            };

            foreach (var signal in _signals)
            {
                if (values.TryGetValue(signal, out var value))
                {
                    row.Add(FormatValue(value));
                }
                else
                {
                    row.Add(string.Empty);
                }
            }
            await _writer.WriteLineAsync(string.Join(COLUMN_DELIMITER, row));
        }

        internal static string FormatValue(object value)
        {
            if (value == null)
                return string.Empty;

            // Handle Dictionary<string, object> (objekty)
            if (value is System.Collections.IDictionary dict)
            {
                var pairs = new List<string>();
                foreach (System.Collections.DictionaryEntry entry in dict)
                {
                    var key = entry.Key?.ToString() ?? "";
                    var val = FormatValue(entry.Value); // Rekurzivní volání
                    pairs.Add($"{key}:{val}");
                }
                return string.Join(LIST_DELIMITER, pairs);
            }

            // Handle List<object> or any IEnumerable
            if (value is System.Collections.IEnumerable enumerable && !(value is string))
            {
                var values = new List<string>();
                foreach (var item in enumerable)
                {
                    if (item == null)
                        continue;

                    // Rekurzivní volání pro každý item (může být Dictionary!)
                    values.Add(FormatValue(item));
                }

                return string.Join(LIST_DELIMITER, values);
            }

            // Handle single numeric values
            if (value is double d2)
                return d2.ToString("G", CultureInfo.InvariantCulture);

            if (value is float f2)
                return f2.ToString("G", CultureInfo.InvariantCulture);

            if (value is int || value is long || value is decimal)
                return Convert.ToString(value, CultureInfo.InvariantCulture);

            return value.ToString();
        }

        public async Task FlushAsync()
        {
            await _writer.FlushAsync();
        }

        public void Flush()
        {
            _writer.Flush();
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _writer?.Dispose();
                _disposed = true;
            }
        }
    }
}
