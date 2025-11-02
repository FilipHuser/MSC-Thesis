using Graphium.Models;

namespace Graphium.Interfaces
{
    internal interface IAppConfigurationService
    {
        #region PROPERTIES
        event EventHandler? ConfigurationChanged;
        #endregion
        #region METHODS
        AppSettings GetAppSettings();
        void SetAppSettings(AppSettings settings);
        #endregion
    }
}
