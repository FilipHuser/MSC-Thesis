using DataHub.Core;
using Graphium.Enums;

namespace Graphium.Services
{
    internal interface ISettingsService
    {
        #region METHODS
        T? Load<T>(SettingsCategory category);
        void Save<T>(T settings, SettingsCategory category , bool append = false);
        #endregion
    }
}
