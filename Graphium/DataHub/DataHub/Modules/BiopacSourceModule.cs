using System.Collections.Concurrent;
using DataHub.Core;
using NLog.Filters;
using SharpPcap;

namespace DataHub.Modules
{
    public class BiopacSourceModule : ModuleBase<RawCapture>
    {
        #region PROPERTIES
        private readonly int _captureDeviceIndex;
        private readonly string? _filter;
        private readonly int? _readTimeout; 
        private ConcurrentQueue<CapturedData<RawCapture>> _dataQueue = [];
        private ILiveDevice? _captureDevice;
        public override ModuleType ModuleType => ModuleType.BIOPAC;
        #endregion
        #region METHODS
        public BiopacSourceModule(int captureDeviceIndex , string? filter = null , int? readTimeout = null)
        {
            _captureDeviceIndex = captureDeviceIndex;
            _filter = filter;
            _readTimeout = readTimeout;
            Init();
        }
        public override IEnumerable<CapturedData<RawCapture>> Get(Func<CapturedData<RawCapture>, bool>? predicate = null, int? skip = null, int? take = null)
        {       
            int skipped = 0;
            int yielded = 0;

            while (_dataQueue.TryDequeue(out var rawItem))
            {
                if (predicate != null && !predicate(rawItem))
                    continue;

                if (skip.HasValue && skipped < skip.Value)
                {
                    skipped++;
                    continue;
                }

                yield return rawItem;
                yielded++;      

                if (take.HasValue && yielded >= take.Value)
                    yield break;
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
        private PosixTimeval? _firstTimeval = null;
        private DateTime _referenceTime;

        protected void device_OnPacketArrival(object sender, PacketCapture pc)
        {
            var packet = pc.GetPacket();

            if (_firstTimeval == null)
            {
                _firstTimeval = packet.Timeval;
                _referenceTime = DateTime.Now;
            }

            // Calculate microseconds offset with proper casting
            long microsecondsOffset =
                ((long)packet.Timeval.Seconds - (long)_firstTimeval.Seconds) * 1_000_000L +
                ((long)packet.Timeval.MicroSeconds - (long)_firstTimeval.MicroSeconds);

            var timestamp = _referenceTime.AddTicks(microsecondsOffset * 10);

            _dataQueue.Enqueue(new CapturedData<RawCapture>(timestamp, packet, this));
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
