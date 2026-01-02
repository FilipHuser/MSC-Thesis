using Graphium.Core;
using Graphium.Models;
using Graphium.ViewModels;

namespace Graphium.Interfaces
{
    internal interface IMeasurementExportService
    {
        #region METHODS
        CsvMeasurementWriter CreateCsvWriter(MeasurementViewModel vm);
        Task<bool> ExportToCsvAsync(IEnumerable<MeasurementDataRow> data, string? defaultFileName = null);
        #endregion
    }
}