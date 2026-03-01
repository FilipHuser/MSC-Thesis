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
        private readonly List<SignalBase> _signals;
        private bool _disposed;
        internal CsvMeasurementWriter(string filePath, List<SignalBase> signals)
        {
            _signals = signals;
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);
            _writer = new StreamWriter(filePath, false, Encoding.UTF8, BUFFER_SIZE);
            WriteHeader();
        }
        private void WriteHeader()
        {
            var headers = new List<string> { "Timestamp" };
            foreach (var signal in _signals)
                headers.Add(signal.Name);
            _writer.WriteLine(string.Join(COLUMN_DELIMITER, headers));
        }
        public async Task WriteRowAsync(DateTime timestamp, Dictionary<SignalBase, object> values)
        {
            long unixTimestampUs = ((DateTimeOffset)timestamp).ToUnixTimeMilliseconds() * 1000
                          + (timestamp.Ticks % TimeSpan.TicksPerMillisecond) / 10;
            var row = new List<string> { unixTimestampUs.ToString(CultureInfo.InvariantCulture) };
            foreach (var signal in _signals)
            {
                if (values.TryGetValue(signal, out var value))
                    row.Add(FormatValue(value));
                else
                    row.Add(string.Empty);
            }
            await _writer.WriteLineAsync(string.Join(COLUMN_DELIMITER, row));
        }
        internal static string FormatValue(object? value)
        {
            if (value is null)
                return string.Empty;
            if (value is System.Collections.IDictionary dict)
            {
                var pairs = new List<string>();
                foreach (System.Collections.DictionaryEntry entry in dict)
                    pairs.Add($"{entry.Key}:{FormatValue(entry.Value)}");
                return string.Join(LIST_DELIMITER, pairs);
            }
            if (value is System.Collections.IEnumerable enumerable && value is not string)
            {
                var items = new List<string>();
                foreach (var item in enumerable)
                    items.Add(FormatValue(item));
                return string.Join(LIST_DELIMITER, items);
            }
            if (value is double d)
                return d.ToString("G", CultureInfo.InvariantCulture);
            if (value is float f)
                return f.ToString("G", CultureInfo.InvariantCulture);
            if (value is int or long or decimal)
                return Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;
            return value.ToString() ?? string.Empty;
        }
        public async Task FlushAsync() => await _writer.FlushAsync();
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