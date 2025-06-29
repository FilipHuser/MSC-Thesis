using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataHub.Core
{
    public abstract class ModuleBase
    {
        #region PROPERTIES
        public bool IsCapturing => _capturingThread?.IsAlive??false;
        protected Thread? _capturingThread;
        #endregion
        #region METHODS
        public abstract void StartCapturing();
        public abstract void StopCapturing();
        public abstract IEnumerable<CapturedData<T>> Get<T>(Func<CapturedData<T>, bool>? predicate = null, int? skip = null, int? take = null);
        #endregion
    }
}