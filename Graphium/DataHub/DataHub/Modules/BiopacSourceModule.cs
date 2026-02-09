using DataHub.Core;
using SharpPcap;

namespace DataHub.Modules
{
    public class BiopacSourceModule : ModuleBase<RawCapture>
    {
        #region PROPERTIES
        private readonly string? _filter;
        private readonly int? _readTimeout;
        private readonly int _captureDeviceIndex;
        private DateTime _referenceTime;
        private ILiveDevice? _captureDevice;
        private PosixTimeval? _firstTimeval;

        public override ModuleType ModuleType => ModuleType.BIOPAC;
        #endregion

        #region METHODS
        public BiopacSourceModule(int captureDeviceIndex, string? filter = null, int? readTimeout = null)
        {
            _captureDeviceIndex = captureDeviceIndex;
            _filter = filter;
            _readTimeout = readTimeout;
            Init();
        }

        private void Init()
        {
            _captureDevice = CaptureDeviceList.Instance.ElementAt(_captureDeviceIndex);
        }

        private void OnPacketArrival(object sender, PacketCapture pc)
        {
            var packet = pc.GetPacket();

            if (_firstTimeval == null)
            {
                _firstTimeval = packet.Timeval;
                _referenceTime = DateTime.Now;
            }

            long microsecondsOffset =
                ((long)packet.Timeval.Seconds - (long)_firstTimeval.Seconds) * 1_000_000L +
                ((long)packet.Timeval.MicroSeconds - (long)_firstTimeval.MicroSeconds);

            var timestamp = _referenceTime.AddTicks(microsecondsOffset * 10);

            Enqueue(new CapturedData<RawCapture>(timestamp, packet, this));
        }

        public override void StartCapturing()
        {
            if (IsCapturing) return;

            _firstTimeval = null;

            _captureDevice?.Open(DeviceModes.Promiscuous, _readTimeout ?? 100);

            if (_captureDevice != null)
            {
                _captureDevice.Filter = _filter ?? "";
                _captureDevice.OnPacketArrival += OnPacketArrival;
            }

            _captureDevice?.StartCapture();

            base.StartCapturing();
        }

        public override void StopCapturing()
        {
            _captureDevice?.StopCapture();

            if (_captureDevice != null)
            {
                _captureDevice.OnPacketArrival -= OnPacketArrival; // ← Odebrat před Close
            }

            _captureDevice?.Close();

            base.StopCapturing();
        }

        public override void Dispose()
        {
            StopCapturing();
            _captureDevice?.Dispose();
        }
        #endregion
    }
}