using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using DataHub.Core;
using DataHub.Modules;

namespace DataHub.Interfaces
{
    public interface IModule : IDisposable
    {
        #region METHODS
        ModuleType ModuleType { get; }
        bool IsCapturing { get; }
        event EventHandler<DataAvailableEventArgs>? DataAvailable;
        #endregion
        #region PROPERTIES
        void StartCapturing();
        void StopCapturing();
        #endregion
    }
}
