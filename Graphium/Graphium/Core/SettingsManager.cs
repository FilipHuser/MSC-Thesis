using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml;

namespace DataHub.Core
{
    public enum SettingsCategory
    {
        SIGNALS,
        USER_SIGNALS_CONFIGURATION
    }
    public static class SettingsManager
    {
        #region PROPERTIES
        private static readonly JsonSerializerOptions _options = new()
        {
            WriteIndented = true,
            IncludeFields = true,
            NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals
        };
        #endregion
        #region METHODS
        private static string GetFilePath(SettingsCategory category)
        {
            var fileName = $"{category.ToString().ToLower()}.json";
            var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) , "Graphium");
            var folderPath = Path.Combine(appDataPath, "Config");

            if(!Directory.Exists(folderPath)) { Directory.CreateDirectory(folderPath); }

            return Path.Combine(folderPath, fileName);
        }
        public static void Save<T>(T settings, SettingsCategory category, bool append = false)
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
        public static T? Load<T>(SettingsCategory category)
        {
            var filePath = GetFilePath(category);
            if (!File.Exists(filePath)) { return default; }
            var json = File.ReadAllText(filePath);
            
            return string.IsNullOrEmpty(json) ? default : JsonSerializer.Deserialize<T>(json, _options);
        }   
        #endregion
    }
}
