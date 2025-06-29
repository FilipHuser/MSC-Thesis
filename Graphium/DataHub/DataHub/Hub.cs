using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataHub.Core;
using DataHub.Modules;
using SharpPcap;

namespace DataHub
{
    public class Hub
    {
        #region PROPERTIES
        public Dictionary<Type , ModuleBase> Modules = new Dictionary<Type, ModuleBase>();
        #endregion
        #region METHODS
        public void AddModule(ModuleBase module) => Modules.Add(module.GetType() , module);
        public void RemoveModule(ModuleBase module) => Modules.Remove(module.GetType());
        public void StartCapturing() => Modules.Values.ToList().ForEach(m => m.StartCapturing());
        public void StopCapturing() => Modules.Values.ToList().ForEach(m => m.StopCapturing());
        #endregion
    };
}
