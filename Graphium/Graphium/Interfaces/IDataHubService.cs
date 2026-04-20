using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataHub.Core;
using DataHub.Interfaces;
using Graphium.Models;

namespace Graphium.Interfaces
{
    interface IDataHubService
    {
        #region METHODS
        Dictionary<ModuleType, List<List<Sample>>> GetData();
        void StartCapturing();
        void StopCapturing();
        bool IsCapturing { get; }
        void AddModule(IModule module);
        void RemoveModule(IModule module);
        double GetSamplingRate(ModuleType moduleType);
        #endregion
    }
}
