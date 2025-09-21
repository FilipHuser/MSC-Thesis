using System.Collections.Concurrent;
using DataHub.Core;
using NLog.Filters;
using SharpPcap;

namespace DataHub.Modules
{
    public class PacketModule : ModuleBase
    {
        #region PROPERTIES
        private readonly int _captureDeviceIndex;
        private readonly string? _filter;
        private readonly int? _readTimeout; 
        private ConcurrentQueue<CapturedData<RawCapture>> _dataQueue = [];
        private ILiveDevice? _captureDevice;
        #endregion
        #region METHODS
        public PacketModule(int captureDeviceIndex , string? filter = null , int? readTimeout = null)
        {
            _captureDeviceIndex = captureDeviceIndex;
            _filter = filter;
            _readTimeout = readTimeout;
            Init();
        }
        public override IEnumerable<CapturedData<T>> Get<T>(Func<CapturedData<T>, bool>? predicate = null, int? skip = null, int? take = null)
        {
            if (typeof(T) != typeof(RawCapture)) { yield break; }            

            int skipped = 0;
            int yielded = 0;

            while (_dataQueue.TryDequeue(out var rawItem))
            {
                var item = (CapturedData<T>)(object)rawItem;

                if (predicate != null && !predicate(item)) { continue; }

                if (skip.HasValue && skipped < skip.Value)
                {
                    skipped++;
                    continue;
                }

                yield return item;
                yielded++;

                if (take.HasValue && yielded >= take.Value) { yield break; }
            }
        }
        private void Init()
        {
            _captureDevice = CaptureDeviceList.Instance.ElementAt(_captureDeviceIndex);
            _captureDevice.OnPacketArrival += new PacketArrivalEventHandler(device_OnPacketArrival);
            _captureDevice.Open(DeviceModes.Promiscuous, _readTimeout ?? 100);
            _captureDevice.Filter = _filter ?? "";
        }
        public override void StartCapturing()
        {
            _captureDevice?.StartCapture();
            base.StartCapturing();
        }
        public override void StopCapturing()
        {
            _captureDevice?.StopCapture();
            base.StopCapturing();
        }
        protected void device_OnPacketArrival(object sender, PacketCapture pc)
        {
            _dataQueue.Enqueue(new CapturedData<RawCapture> (DateTime.Now, pc.GetPacket() , this));
        }
        protected override Task CaptureTask(CancellationToken ct)
        {
            return Task.CompletedTask;
        }
        public override void Dispose()
        {
            StopCapturing();
            _captureDevice?.Close();
            _captureDevice?.Dispose();
        }
        #endregion
    }
}
