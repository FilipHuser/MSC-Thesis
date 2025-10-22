namespace Graphium.Interfaces
{
    internal interface ILoggingService
    {
        #region PROPERTIES
        void LogInfo(string message);
        void LogDebug(string message);
        void LogWarning(string message);
        void LogError(string message);
        #endregion
        #region METHODS
        #endregion
    }
}
