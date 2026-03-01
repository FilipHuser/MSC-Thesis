using System.Globalization;
using System.IO;
using System.Text;
using Graphium.Core;
using Graphium.Interfaces;
using Graphium.Models;
using Graphium.ViewModels;

namespace Graphium.Services
{
    internal class MeasurementExportService : IMeasurementExportService
    {
        #region PROPERTIES
        private readonly ISignalService _signalService;
        private readonly IFileExportService _fileExportService;
        #endregion
        #region METHODS
        public MeasurementExportService(ISignalService signalService, IFileExportService fileExportService)
        {
            _signalService = signalService;
            _fileExportService = fileExportService;
        }
        public CsvMeasurementWriter CreateCsvWriter(MeasurementViewModel vm)
        {
            var signals = _signalService.Signals?.OrderBy(s => s.Name).ToList()
              ?? new List<SignalBase>();
            if (signals.Count == 0)
                throw new InvalidOperationException("No signals available for export.");
            var fileName = $"{vm.Name}_tmp.csv";
            var filePath = GetMeasurementFilePath(fileName);
            return new CsvMeasurementWriter(filePath, signals);
        }
        public async Task SaveAsync(MeasurementViewModel vm)
        {
            var fileName = $"{vm.Name}_tmp.csv";
            var sourcePath = GetMeasurementFilePath(fileName);
            await _fileExportService.ExportAsync(
                sourcePath,
                $"{vm.Name}.csv",
                "CSV files (*.csv)|*.csv|All files (*.*)|*.*");
        }
        private static string GetMeasurementFilePath(string fileName)
        {
            var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Graphium");
            var folderPath = Path.Combine(appDataPath, "Measurements");
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
            return Path.Combine(folderPath, fileName);
        }
        private void WriteCsvFile(string filePath, List<SignalBase> signals, IEnumerable<MeasurementDataRow> data)
        {
            using var writer = new StreamWriter(filePath, false, Encoding.UTF8, CsvMeasurementWriter.BUFFER_SIZE);
            writer.WriteLine(BuildCsvHeader(signals));
            foreach (var row in data)
                writer.WriteLine(BuildCsvRow(signals, row));
        }
        private string BuildCsvHeader(List<SignalBase> signals)
        {
            var headers = new List<string> { "Timestamp" };
            foreach (var signal in signals)
                headers.Add(signal.Name);
            return string.Join(CsvMeasurementWriter.COLUMN_DELIMITER, headers);
        }
        private string BuildCsvRow(List<SignalBase> signals, MeasurementDataRow row)
        {
            var values = new List<string>
        {
            row.Timestamp.ToString("F3", CultureInfo.InvariantCulture)
        };
            foreach (var signal in signals)
            {
                if (row.Values.TryGetValue(signal, out var value))
                    values.Add(CsvMeasurementWriter.FormatValue(value));
                else
                    values.Add(string.Empty);
            }
            return string.Join(CsvMeasurementWriter.COLUMN_DELIMITER, values);
        }
        #endregion
    }
}