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
        private readonly Dictionary<Signal, List<double>> _lastKnownValues = new();
        public List<Signal> Signals { get; set; } = [];
        private readonly string _delimiter;
        private readonly CultureInfo _invariantCulture = CultureInfo.InvariantCulture;
        private readonly object _exportLock = new object();

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

            _fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 16 * 1024, useAsync: true);

            _streamWriter = new StreamWriter(_fileStream, Encoding.UTF8) { AutoFlush = false };
            Signals = measurement.Signals.ToList();

            // Initialize tracking for each signal
            foreach (var signal in Signals)
            {
                _lastWrittenIndexPerSignal[signal] = 0;
                _lastKnownValues[signal] = new List<double>();
            }
        }

        public async Task ExportAsync()
        {
            // Thread-safe snapshot of signal data
            var signalSnapshots = new Dictionary<Signal, (List<double> XData, List<List<double>> YData, int StartIndex)>();
            
            lock (_exportLock)
            {
                // Write header if not already written
                if (!_headerWritten)
                {
                    WriteHeaderSync();
                    _headerWritten = true;
                }

                // Create snapshots of all signal data - only copy NEW data since last export
                foreach (var signal in Signals)
                {
                    int startIndex = _lastWrittenIndexPerSignal[signal];
                    
                    // Only copy the slice of data we haven't exported yet
                    var xDataCopy = signal.XData != null && signal.XData.Count > startIndex
                        ? signal.XData.Skip(startIndex).ToList()
                        : new List<double>();
                    
                    var yDataCopy = signal.YData != null
                        ? signal.YData.Select(ch => ch.Count > startIndex ? ch.Skip(startIndex).ToList() : new List<double>()).ToList()
                        : new List<List<double>>();
                    
                    signalSnapshots[signal] = (xDataCopy, yDataCopy, startIndex);
                }
            }

            // Collect all new unique timestamps from all signals
            var timestampToValues = new SortedDictionary<double, Dictionary<Signal, List<double>>>();

            foreach (var signal in Signals)
            {
                var (xData, yData, startIndex) = signalSnapshots[signal];
                
                if (xData == null || yData == null || xData.Count == 0) continue;

                // Process only new data points (already sliced in snapshot)
                for (int i = 0; i < xData.Count; i++)
                {
                    double timestamp = xData[i];

                    if (!timestampToValues.ContainsKey(timestamp))
                    {
                        timestampToValues[timestamp] = new Dictionary<Signal, List<double>>();
                    }

                    // Collect all channel values for this signal at this timestamp
                    var channelValues = new List<double>(yData.Count); // Pre-allocate capacity
                    for (int ch = 0; ch < yData.Count; ch++)
                    {
                        if (i < yData[ch].Count)
                        {
                            channelValues.Add(yData[ch][i]);
                        }
                    }

                    if (channelValues.Count > 0)
                    {
                        timestampToValues[timestamp][signal] = channelValues;
                        // Update last known values for this signal
                        _lastKnownValues[signal] = channelValues;
                    }
                }

                // Update the last written index for this signal
                lock (_exportLock)
                {
                    _lastWrittenIndexPerSignal[signal] = startIndex + xData.Count;
                }
            }

            // Track last value for each signal for padding (initialized from _lastKnownValues)
            var lastValues = new Dictionary<Signal, List<double>>();
            foreach (var signal in Signals)
            {
                // Only initialize if we have actual data from previous exports
                if (_lastKnownValues.ContainsKey(signal) && _lastKnownValues[signal].Count > 0)
                {
                    lastValues[signal] = new List<double>(_lastKnownValues[signal]);
                }
            }

            // If no new data for any signal, skip export
            if (timestampToValues.Count == 0)
            {
                return;
            }

            // Write rows for each unique timestamp
            var rowBuffer = new StringBuilder(256); // Pre-allocate with reasonable size
            
            foreach (var kvp in timestampToValues)
            {
                double timestamp = kvp.Key;
                var valuesAtTime = kvp.Value;

                rowBuffer.Clear();
                // Use invariant culture for decimal point '.'
                rowBuffer.Append(timestamp.ToString(TimestampFormat, _invariantCulture));

                foreach (var signal in Signals)
                {
                    rowBuffer.Append(_delimiter); // Add the main column delimiter

                    if (valuesAtTime.TryGetValue(signal, out var channelValues) && channelValues.Count > 0)
                    {
                        // Signal has data at this timestamp - use it and update last values
                        lastValues[signal] = channelValues;

                        // Use a comma-separated format for multi-channel data within the single column
                        for (int i = 0; i < channelValues.Count; i++)
                        {
                            // Use invariant culture for decimal point '.'
                            rowBuffer.Append(channelValues[i].ToString(ValueFormat, _invariantCulture));
                            if (i < channelValues.Count - 1)
                            {
                                rowBuffer.Append(','); // Use comma as the inner channel delimiter
                            }
                        }
                    }
                    else
                    {
                        // Signal doesn't have data at this timestamp - pad with last known values
                        if (lastValues.TryGetValue(signal, out var lastChannelValues) && lastChannelValues.Count > 0)
                        {
                            // Use a comma-separated format for multi-channel data within the single column
                            for (int i = 0; i < lastChannelValues.Count; i++)
                            {
                                // Use invariant culture for decimal point '.'
                                rowBuffer.Append(lastChannelValues[i].ToString(ValueFormat, _invariantCulture));
                                if (i < lastChannelValues.Count - 1)
                                {
                                    rowBuffer.Append(','); // Use comma as the inner channel delimiter
                                }
                            }
                        }
                        // else: Signal hasn't started yet - leave column empty (no padding before first value)
                    }
                }

                await _streamWriter.WriteLineAsync(rowBuffer.ToString());
            }

            await _streamWriter.FlushAsync();
        }

        private void WriteHeaderSync()
        {
            var header = new StringBuilder("Timestamp");
            foreach (var signal in Signals)
            {
                header.Append(_delimiter);
                header.Append(signal.Name ?? $"Signal_{signal.Source}");
            }
            _streamWriter.WriteLine(header.ToString());
        }

        public void Dispose()
        {
            _streamWriter?.Dispose();
            _fileStream?.Dispose();
        }
        #endregion
    }
}