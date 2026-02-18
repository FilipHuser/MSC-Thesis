using DataHub.Core;

namespace DataHub.Interfaces
{
    public interface IModule : IDisposable
    {
        #region METHODS
        bool IsCapturing { get; }
        double SamplingRate { get; }
        ModuleType ModuleType { get; }
        event EventHandler<DataAvailableEventArgs>? DataAvailable;
        #endregion
        #region PROPERTIES
        void StartCapturing();
        void StopCapturing();
        #endregion
    }
}
