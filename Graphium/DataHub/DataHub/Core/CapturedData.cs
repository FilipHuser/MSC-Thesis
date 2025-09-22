using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataHub.Core
{
    public sealed record CapturedData<T>
    {
        #region PROPERTIES
        public DateTime Timestamp;
        public ModuleBase<T>? Caller;
        public T Data;
        #endregion
        #region METHODS
        public CapturedData(DateTime timestamp, T data, ModuleBase<T> caller)
        {
            Timestamp = timestamp;
            Caller = caller;
            Data = data;
        }
        #endregion
    }
}
