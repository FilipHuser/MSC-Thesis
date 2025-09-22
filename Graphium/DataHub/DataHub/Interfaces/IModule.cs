using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataHub.Interfaces
{
    public interface IModule
    {
        void StartCapturing();
        void StopCapturing();
        bool IsCapturing { get; }
    }
}
