using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataHub.Interfaces;

namespace DataHub.Core
{
    public abstract class ModuleBase<T> : IModule
    {
        #region PROPERTIES
        private Task? _capturingTask;
        private CancellationTokenSource? _cts;
        public bool IsCapturing { get; set; }
        public abstract ModuleType ModuleType { get; }
        public event EventHandler<DataAvailableEventArgs>? DataAvailable;
        protected ConcurrentQueue<CapturedData<T>> DataQueue { get; } = new();
        #endregion
        #region METHODS
        protected virtual Task CaptureTask(CancellationToken ct) => Task.CompletedTask;
        protected void Enqueue(CapturedData<T> data)
        {
            DataQueue.Enqueue(data);
            DataAvailable?.Invoke(this, new DataAvailableEventArgs(ModuleType));
        }
        public virtual void StartCapturing()
        {
            if (IsCapturing) return;
            IsCapturing = true;
            _cts = new CancellationTokenSource();
            _capturingTask = Task.Run(async () =>
            {
                try
                {
                    await CaptureTask(_cts.Token);
                }
                catch (OperationCanceledException) { }
                finally
                {
                    IsCapturing = false;
                }
            });
        }
        public virtual async void StopCapturing()
        {
            if (!IsCapturing) return;
            _cts?.Cancel();
            if (_capturingTask != null)
            {
                try { await _capturingTask; }
                catch (OperationCanceledException) { }
            }
            IsCapturing = false;
        }
        public IEnumerable<CapturedData<T>> Get(Func<CapturedData<T>, bool>? predicate = null, int? skip = null, int? take = null)
        {
            int skipped = 0;
            int yielded = 0;
            while (DataQueue.TryDequeue(out var item))
            {
                if (predicate != null && !predicate(item))
                    continue;
                if (skip.HasValue && skipped < skip.Value)
                {
                    skipped++;
                    continue;
                }
                yield return item;
                yielded++;
                if (take.HasValue && yielded >= take.Value)
                    yield break;
            }
        }
        public virtual void Dispose()
        {
            StopCapturing();
            _cts?.Dispose();
        }
        #endregion
    }
}