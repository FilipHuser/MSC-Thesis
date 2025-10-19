using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataHub.Interfaces;

namespace Graphium.Interfaces
{
    interface IDataHubService
    {
        #region METHODS
        void StartCapturing();
        void StopCapturing();
        void AddModule(IModule module);
        void RemoveModule(IModule module);
        #endregion
    }
}
