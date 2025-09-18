using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataHub.Core
{
    public abstract class ModuleBase : IDisposable
    {
        #region PROPERTIES
        private CancellationTokenSource? _cts;
        private Task? _capturingTask;
        public bool IsCapturing { get; set; }
        #endregion
        #region METHODS
        protected abstract Task CaptureTask(CancellationToken ct);
        public virtual void StartCapturing()
        {
            if (IsCapturing) { return; }
            _cts = new CancellationTokenSource();
            _capturingTask = Task.Run(async () =>
            {
                try {
                    await CaptureTask(_cts.Token);
                } catch (OperationCanceledException) {
                    // NLOG
                } finally {
                    IsCapturing = false; 
                }
            });
        }
        public virtual async void StopCapturing()
        {
            if(!IsCapturing) { return; }
            _cts?.Cancel();
            if (_capturingTask != null) { await _capturingTask; }
        }
        public abstract IEnumerable<CapturedData<T>> Get<T>(Func<CapturedData<T>, bool>? predicate = null, int? skip = null, int? take = null);
        public virtual void Dispose()
        {
            StopCapturing();
            _cts?.Dispose();
            _capturingTask?.Dispose();
        }
        #endregion
    }
}