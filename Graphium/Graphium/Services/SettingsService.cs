using System.IO;
using Graphium.Enums;
using System.Text.Json;
using Graphium.Interfaces;
using System.Text.Json.Serialization;

namespace Graphium.Services
{
    internal class SettingsService : ISettingsService
    {
        #region SERVICES
        private readonly ILoggingService _loggingService;
        #endregion
        #region PROPERTIES
        private static readonly JsonSerializerOptions _options = new()
        {
            WriteIndented = true,
            IncludeFields = true,
            NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals
        };
        #endregion
        #region METHODS
        public SettingsService(ILoggingService loggingService)
        {
            _loggingService = loggingService;
        }
        public T? Load<T>(SettingsCategory category)
        {
            try
            {
                var filePath = GetFilePath(category);
                if (!File.Exists(filePath))
                    return default;

                var json = File.ReadAllText(filePath);
                if (string.IsNullOrEmpty(json))
                    return default;

                return JsonSerializer.Deserialize<T>(json, _options);
            }
            catch (IOException ex)
            {
                _loggingService.LogError($"I/O error loading settings: {ex.Message}");
                return default;
            }
            catch (JsonException ex)
            {
                _loggingService.LogError($"JSON parsing error: {ex.Message}");
                return default;
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Unexpected error: {ex.Message}");
                return default;
            }
        }
        public void Save<T>(T settings, SettingsCategory category, bool append = false)
        {
            var filePath = GetFilePath(category);

            if (append && File.Exists(filePath))
            {
                var existingJson = File.ReadAllText(filePath);
                var existingData = JsonSerializer.Deserialize<List<T>>(existingJson, _options) ?? new List<T>();

                if (settings is IEnumerable<T> newItems)
                {
                    existingData.AddRange(newItems);
                }
                else
                {
                    existingData.Add(settings);
                }

                var json = JsonSerializer.Serialize(existingData, _options);
                File.WriteAllText(filePath, json);
            }
            else
            {
                var json = JsonSerializer.Serialize(settings, _options);
                File.WriteAllText(filePath, json);
            }
        }
        private static string GetFilePath(SettingsCategory category)
        {
            var fileName = $"{category.ToString().ToLower()}.json";
            var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Graphium");
            var folderPath = Path.Combine(appDataPath, "Config");

            if (!Directory.Exists(folderPath)) { Directory.CreateDirectory(folderPath); }

            return Path.Combine(folderPath, fileName);
        }
        #endregion
    }
}
