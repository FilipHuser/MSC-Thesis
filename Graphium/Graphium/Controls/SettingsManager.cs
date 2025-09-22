using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
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
            IncludeFields = true
        };
        #endregion
        #region METHODS
        private static string GetFilePath(SettingsCategory category)
        {
            var fileName = $"{category.ToString().ToLower()}.json";
            var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) , "Graphium");

            if(!Directory.Exists(appDataPath)) { Directory.CreateDirectory(appDataPath); }

            return Path.Combine(appDataPath, fileName);
        }
        public static void Save<T>(T settings , SettingsCategory category)
        {
            var json = JsonSerializer.Serialize(settings, _options);
            File.WriteAllText(GetFilePath(category), json);
        }
        public static T? Load<T>(SettingsCategory category)
        {
            var filePath = GetFilePath(category);
            if (!File.Exists(filePath)) { return default; }
            var json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<T>(json, _options);
        }   
        #endregion
    }
}
