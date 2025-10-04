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
    [JsonDerivedType(typeof(BiopacSourceModule), "BiopacSourceModule")]
    [JsonDerivedType(typeof(VRSourceModule), "VRSourceModule")]
    public interface IModule
    {
        void StartCapturing();
        void StopCapturing();
        ModuleType ModuleType { get; }
        bool IsCapturing { get; }
    }
}
