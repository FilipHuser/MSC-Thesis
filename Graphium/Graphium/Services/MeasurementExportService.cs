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
        private readonly IDialogService _dialogService;
        #endregion
        #region CONSTRUCTOR
        public MeasurementExportService(ISignalService signalService, IDialogService dialogService)
        {
            _signalService = signalService;
            _dialogService = dialogService;
        }
        #endregion
        #region CSV EXPORT METHODS
        public CsvMeasurementWriter CreateCsvWriter(MeasurementViewModel vm)
        {
            var signals = _signalService.Signals?.OrderBy(s => s.Name).ToList()
              ?? new List<Signal>();

            if (signals.Count == 0)
            {
                throw new InvalidOperationException("No signals available for export.");
            }



            var fileName = $"{vm.Name}.measurement.tmp.csv";
            var filePath = GetMeasurementFilePath(fileName);

            return new CsvMeasurementWriter(filePath, signals);
        }

        public async Task<bool> ExportToCsvAsync(IEnumerable<MeasurementDataRow> data, string defaultFileName = null)
        {
            defaultFileName ??= $"Measurements_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            var filePath = _dialogService.ShowSaveFile("CSV files (*.csv)|*.csv", defaultFileName);

            if (string.IsNullOrEmpty(filePath))
                return false;

            try
            {
                var signals = _signalService.Signals?.OrderBy(s => s.Name).ToList();
                if (signals == null || signals.Count == 0)
                {
                    _dialogService.ShowWarning("No signals available for export.");
                    return false;
                }

                await Task.Run(() => WriteCsvFile(filePath, signals, data));
                _dialogService.ShowInfo("Measurements exported successfully.");
                return true;
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Export failed: {ex.Message}");
                return false;
            }
        }

        #endregion
        #region PRIVATE HELPERS

        private static string GetMeasurementFilePath(string fileName)
        {
            var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Graphium");
            var folderPath = Path.Combine(appDataPath, "Measurements");

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            return Path.Combine(folderPath, fileName);
        }

        private void WriteCsvFile(string filePath, List<Signal> signals, IEnumerable<MeasurementDataRow> data)
        {
            using var writer = new StreamWriter(filePath, false, Encoding.UTF8, CsvMeasurementWriter.BUFFER_SIZE);

            // Write header
            writer.WriteLine(BuildCsvHeader(signals));

            // Write data rows
            foreach (var row in data)
            {
                writer.WriteLine(BuildCsvRow(signals, row));
            }
        }

        private string BuildCsvHeader(List<Signal> signals)
        {
            var headers = new List<string> { "Timestamp" };

            foreach (var signal in signals)
            {
                headers.Add(signal.Name);
            }

            return string.Join(CsvMeasurementWriter.COLUMN_DELIMITER, headers);
        }

        private string BuildCsvRow(List<Signal> signals, MeasurementDataRow row)
        {
            var values = new List<string>
            {
                row.Timestamp.ToString("F3", CultureInfo.InvariantCulture)
            };

            foreach (var signal in signals)
            {
                if (row.Values.TryGetValue(signal, out var value))
                {
                    values.Add(CsvMeasurementWriter.FormatValue(value));
                }
                else
                {
                    values.Add(string.Empty);
                }
            }

            return string.Join(CsvMeasurementWriter.COLUMN_DELIMITER, values);
        }

        #endregion
    }
}