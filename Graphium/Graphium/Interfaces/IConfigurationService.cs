using DataHub.Core;
using Graphium.Enums;

namespace Graphium.Interfaces
{
    internal interface IConfigurationService
    {
        #region METHODS
        T? Load<T>(SettingsCategory category);
        void Save<T>(T settings, SettingsCategory category, bool append = false);
        #endregion
    }
}
