using DataHub.Core;
using Graphium.Interfaces;
using Graphium.Models;
using System.Text.Json;

namespace Graphium.Services
{
    internal class DataExportService : IDataExportService, IDisposable
    {
        private readonly IAppConfigurationService _appConfigurationService;
        private UdpExporter? _exporter;
        public bool IsEnabled { get; set; } = false;

        public DataExportService(IAppConfigurationService appConfigurationService)
        {
            _appConfigurationService = appConfigurationService;
        }
        public void Start()
        {
            var settings = _appConfigurationService.GetAppSettings();
            IsEnabled = settings.ExportEnabled;
            if (!IsEnabled) return;
            _exporter?.Dispose();
            _exporter = new UdpExporter(settings.ExportHost, settings.ExportPort);
        }
        public void Stop()
        {
            _exporter?.Dispose();
            _exporter = null;
        }
        public void Export(Dictionary<SignalBase, object> rowValues, double timestamp)
        {
            if (!IsEnabled || _exporter == null) return;
            var payload = new
            {
                t = timestamp,
                s = rowValues.ToDictionary(k => k.Key.Name, v => v.Value)
            };
            _exporter.Send(JsonSerializer.Serialize(payload));
        }
        public void Dispose() => Stop();
    }
}