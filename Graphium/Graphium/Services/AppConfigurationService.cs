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
        private AppSettings _settings { get; set; } = new();
        public event EventHandler? ConfigurationChanged;
        #endregion
        #region METHODS
        public AppConfigurationService(IConfigurationService configurationService)
        {
            _configurationService = configurationService;

            var settings = _configurationService.Load<AppSettings>(SettingsCategory.APP_CONFIGURATION);

            if (settings == null)
            {
                _settings = new AppSettings();
                _configurationService.Save(_settings, SettingsCategory.APP_CONFIGURATION);
            }
        }
        public AppSettings GetAppSettings() => _settings;
        public void SetAppSettings(AppSettings settings)
        {
            _settings = settings;
            _configurationService.Save(_settings, SettingsCategory.APP_CONFIGURATION);
            ConfigurationChanged?.Invoke(this, EventArgs.Empty);
        }
        #endregion
    }
}
