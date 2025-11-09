using System.Runtime;
using Graphium.Models;
using Graphium.Enums;
using Graphium.Interfaces;

namespace Graphium.Services
{
    internal class AppConfigurationService : IAppConfigurationService
    {
        #region SERVICES
        private readonly IConfigurationService _configurationService;
        #endregion
        #region PROPERTIES
        public event EventHandler? ConfigurationChanged;
        #endregion
        #region METHODS
        public AppConfigurationService(IConfigurationService configurationService)
        {
            _configurationService = configurationService;
        }
        public AppSettings GetAppSettings()
        {
            var settings = _configurationService.Load<AppSettings>(SettingsCategory.APP_CONFIGURATION);

            if (settings != null)
                return settings;

            var defaultSettings = new AppSettings();
            _configurationService.Save(defaultSettings, SettingsCategory.APP_CONFIGURATION);

            return defaultSettings;
        }
        public void SetAppSettings(AppSettings settings)
        {
            _configurationService.Save(settings, SettingsCategory.APP_CONFIGURATION);
            ConfigurationChanged?.Invoke(this, EventArgs.Empty);
        }
        #endregion
    }
}
