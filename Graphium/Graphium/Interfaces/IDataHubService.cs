using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataHub.Core;
using DataHub.Interfaces;

namespace Graphium.Interfaces
{
    interface IDataHubService
    {
        #region METHODS
        Dictionary<ModuleType, Dictionary<int, List<(object value, DateTime timestamp)>>?> GetData();
        void StartCapturing();
        void StopCapturing();
        bool IsCapturing { get; }
        void AddModule(IModule module);
        void RemoveModule(IModule module);
        #endregion
    }
}
