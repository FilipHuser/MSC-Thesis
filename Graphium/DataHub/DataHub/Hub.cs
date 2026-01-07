using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataHub.Core;
using DataHub.Interfaces;
using DataHub.Modules;
using SharpPcap;

namespace DataHub
{
    public class Hub
    {
        #region PROPERTIES
        public bool IsCapturing { get; private set; }
        public Dictionary<Type , IModule> Modules = new Dictionary<Type, IModule>();
        #endregion
        #region METHODS
        public void AddModule(IModule module) => Modules.Add(module.GetType() , module);
        public void RemoveModule(IModule module) => Modules.Remove(module.GetType());
        public void StartCapturing()
        {
            Modules.Values.ToList().ForEach(m => m.StartCapturing());
            IsCapturing = true;
        }
        public void StopCapturing()
        {
            Modules.Values.ToList().ForEach(m => m.StopCapturing());
            IsCapturing = false;
        }
        public void ClearModules()
        {
            if (IsCapturing) { StopCapturing(); }
            Modules.Clear();
        }
        #endregion
    };
}
