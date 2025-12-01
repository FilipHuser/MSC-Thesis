using Graphium.Models;
using Graphium.ViewModels;
using System.Globalization;
using System.IO;
using System.Text;

internal class CsvDataExporter : IDisposable
{
    #region PROPERTIES
    private readonly FreezeFrame _freezeFrame;
    private readonly List<Signal> _signals;
    private readonly char _delimiter = ';';
    private readonly char _multiValueDelimiter = ',';
    private const string ValueFormat = "G17";
    private const string TimestampFormat = "F6";
    private readonly CultureInfo _invariantCulture = CultureInfo.InvariantCulture;

    private readonly FileStream _fileStream;
    private readonly StreamWriter _streamWriter;
    private bool _headerWritten = false;

    public int ExportIntervalMs { get; }
    public bool ExportOnUpdate { get; }
    #endregion

    #region CONSTRUCTOR
    public CsvDataExporter(
        FreezeFrame freezeFrame,
        MeasurementViewModel measurement,
        int exportIntervalMs,
        bool exportOnUpdate = false)
    {
        _freezeFrame = freezeFrame;
        _signals = measurement.Signals.ToList();
        ExportIntervalMs = exportIntervalMs;
        ExportOnUpdate = exportOnUpdate;

        var fileName = $"{measurement.Name}_tmpMeasurement.csv";
        var appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Graphium"
        );
        var measurementsFolderPath = Path.Combine(appDataPath, "Measurements");
        Directory.CreateDirectory(measurementsFolderPath);
        var filePath = Path.Combine(measurementsFolderPath, fileName);

        _fileStream = new FileStream(
            filePath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 16 * 1024,
            useAsync: false
        );
        _streamWriter = new StreamWriter(_fileStream, Encoding.UTF8);

        WriteHeader();
    }
    #endregion

    #region METHODS
    private void WriteHeader()
    {
        if (_headerWritten) return;

        var header = new StringBuilder("Timestamp");

        foreach (var signal in _signals)
        {
            header.Append(_delimiter);
            header.Append(signal.Name ?? $"Signal_{signal.Source}");
        }

        _streamWriter.WriteLine(header.ToString());
        _streamWriter.Flush();
        _headerWritten = true;
    }

    // ✅ NOVÉ: Export s custom timestampem
    public void TriggerExport(double? customTimestamp = null)
    {
        if (!_headerWritten) return;

        var snapshot = _freezeFrame.GetSnapshot();
        double timestamp = customTimestamp ?? _freezeFrame.GetMaxTimestamp();

        WriteRow(timestamp, snapshot);
    }

    private void WriteRow(double timestamp, Dictionary<Signal, (double timestamp, object value)> snapshot)
    {
        var sb = new StringBuilder();

        // Timestamp
        sb.Append(timestamp.ToString(TimestampFormat, _invariantCulture));

        // Values for each signal
        foreach (var signal in _signals)
        {
            sb.Append(_delimiter);

            if (snapshot.TryGetValue(signal, out var data) && data.value != null)
            {
                if (data.value is IEnumerable<object> channelData && data.value is not string)
                {
                    var values = channelData.Select(v => Convert.ToDouble(v).ToString(ValueFormat, _invariantCulture));
                    sb.Append(string.Join(_multiValueDelimiter.ToString(), values));
                }
                else
                {
                    sb.Append(Convert.ToDouble(data.value).ToString(ValueFormat, _invariantCulture));
                }
            }
        }

        _streamWriter.WriteLine(sb.ToString());
        _fileStream.Flush();
    }

    public async Task RunPeriodicExportAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            TriggerExport();
            await Task.Delay(ExportIntervalMs, token);
        }
    }

    public void Dispose()
    {
        _streamWriter?.Flush();
        _streamWriter?.Dispose();
        _fileStream?.Dispose();
    }
    #endregion
}