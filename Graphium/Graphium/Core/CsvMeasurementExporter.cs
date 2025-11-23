using System.IO;
using System.Text;
using System.Globalization; // Needed for CultureInfo.InvariantCulture
using Graphium.Models;
using Graphium.ViewModels;

namespace Graphium.Core
{
    internal class CsvMeasurementExporter : IDisposable
    {
        #region PROPERTIES
        private readonly FileStream _fileStream;
        private readonly StreamWriter _streamWriter;
        private bool _headerWritten = false;
        private readonly Dictionary<Signal, int> _lastWrittenIndexPerSignal = new();
        public List<Signal> Signals { get; set; } = [];
        private readonly string _delimiter;
        private readonly CultureInfo _invariantCulture = CultureInfo.InvariantCulture;

        private const string TimestampFormat = "F3";
        private const string ValueFormat = "G17";
        #endregion

        #region METHODS
        public CsvMeasurementExporter(MeasurementViewModel measurement, string delimiter = ";")
        {
            _delimiter = delimiter;
            var fileName = $"{measurement.Name}_tmpMeasurement.csv";
            var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Graphium");
            var measurementsFolderPath = Path.Combine(appDataPath, "Measurements");
            Directory.CreateDirectory(measurementsFolderPath);
            var filePath = Path.Combine(measurementsFolderPath, fileName);

            _fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write,
                FileShare.None, bufferSize: 16 * 1024, useAsync: true);

            // Removed AutoFlush = true for performance
            _streamWriter = new StreamWriter(_fileStream, Encoding.UTF8) { AutoFlush = false };
            Signals = measurement.Signals.ToList();

            // Initialize tracking for each signal
            foreach (var signal in Signals)
            {
                _lastWrittenIndexPerSignal[signal] = 0;
            }
        }

        public async Task ExportAsync()
        {
            // Write header if not already written
            if (!_headerWritten)
            {
                await WriteHeaderAsync();
                _headerWritten = true;
            }

            // Collect all new unique timestamps from all signals
            // NOTE: This remains the primary performance bottleneck for large data sets
            var timestampToValues = new SortedDictionary<double, Dictionary<Signal, List<double>>>();

            foreach (var signal in Signals)
            {
                if (signal.XData == null || signal.YData == null) continue;

                int startIndex = _lastWrittenIndexPerSignal[signal];

                // Process only new data points since last export for this signal
                for (int i = startIndex; i < signal.XData.Count; i++)
                {
                    double timestamp = signal.XData[i];

                    if (!timestampToValues.ContainsKey(timestamp))
                    {
                        timestampToValues[timestamp] = new Dictionary<Signal, List<double>>();
                    }

                    // Collect all channel values for this signal at this timestamp
                    var channelValues = new List<double>();
                    foreach (var channel in signal.YData)
                    {
                        if (i < channel.Count)
                        {
                            channelValues.Add(channel[i]);
                        }
                    }

                    timestampToValues[timestamp][signal] = channelValues;
                }

                // Update the last written index for this signal
                _lastWrittenIndexPerSignal[signal] = signal.XData.Count;
            }

            // Track last value for each signal for padding
            var lastValues = new Dictionary<Signal, List<double>>();

            // Write rows for each unique timestamp
            foreach (var kvp in timestampToValues)
            {
                double timestamp = kvp.Key;
                var valuesAtTime = kvp.Value;

                var row = new StringBuilder();
                // Use invariant culture for decimal point '.'
                row.Append(timestamp.ToString(TimestampFormat, _invariantCulture));

                foreach (var signal in Signals)
                {
                    row.Append(_delimiter); // Add the main column delimiter

                    if (valuesAtTime.TryGetValue(signal, out var channelValues))
                    {
                        // Signal has data at this timestamp - use it and update last values
                        lastValues[signal] = channelValues;

                        // Use a comma-separated format for multi-channel data within the single column
                        for (int i = 0; i < channelValues.Count; i++)
                        {
                            // Use invariant culture for decimal point '.'
                            row.Append(channelValues[i].ToString(ValueFormat, _invariantCulture));
                            if (i < channelValues.Count - 1)
                            {
                                row.Append(','); // Use comma as the inner channel delimiter
                            }
                        }
                    }
                    else
                    {
                        // Signal doesn't have data at this timestamp - pad with last known values
                        if (lastValues.TryGetValue(signal, out var lastChannelValues))
                        {
                            // Use a comma-separated format for multi-channel data within the single column
                            for (int i = 0; i < lastChannelValues.Count; i++)
                            {
                                // Use invariant culture for decimal point '.'
                                row.Append(lastChannelValues[i].ToString(ValueFormat, _invariantCulture));
                                if (i < lastChannelValues.Count - 1)
                                {
                                    row.Append(','); // Use comma as the inner channel delimiter
                                }
                            }
                        }
                        else
                        {
                            // No data yet for this signal - write an empty column value
                            // For multi-channel signals, this results in an empty string in the column.
                            // If you need padding for all channels (e.g., ",," for 3 channels), 
                            // the header logic would need to change, but given the 
                            // single-column requirement, an empty string is appropriate.
                        }
                    }
                }

                await _streamWriter.WriteLineAsync(row.ToString());
            }

            await _streamWriter.FlushAsync();
        }

        private async Task WriteHeaderAsync()
        {
            var header = new StringBuilder("Timestamp");
            foreach (var signal in Signals)
            {
                header.Append(_delimiter);

                header.Append(signal.Name ?? $"Signal_{signal.Source}");
            }
            await _streamWriter.WriteLineAsync(header.ToString());
        }

        public void Dispose()
        {
            _streamWriter?.Dispose();
            _fileStream?.Dispose();
        }
        #endregion
    }
}