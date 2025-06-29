using System.Collections.Concurrent;
using DataHub.Core;
using SharpPcap;

namespace DataHub.Modules
{
    public class PacketModule : ModuleBase
    {
        #region PROPERTIES
        private ConcurrentQueue<CapturedData<RawCapture>> _dataQueue = [];
        private readonly ILiveDevice _captureDevice;
        #endregion
        #region METHODS
        public PacketModule(int captureDeviceIndex , string? filter = null , int? readTimeout = null)
        {
            _captureDevice = CaptureDeviceList.Instance.ElementAt(captureDeviceIndex);
            _captureDevice.OnPacketArrival +=  new PacketArrivalEventHandler(device_OnPacketArrival);
            _captureDevice.Open(DeviceModes.Promiscuous , readTimeout??100);
            _captureDevice.Filter = filter??"";
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
        public override void StartCapturing()
        {
            _capturingThread = new Thread(() => { _captureDevice.StartCapture(); });
            _capturingThread.IsBackground = true;
            _capturingThread.Start();
        }
        public override void StopCapturing()
        {
            _captureDevice.StopCapture();
            _captureDevice.Close();
            _capturingThread?.Join();
        }
        protected void device_OnPacketArrival(object sender, PacketCapture pc)
        {
            _dataQueue.Enqueue(new CapturedData<RawCapture> (DateTime.Now, pc.GetPacket() , this));
        }
        #endregion
    }
}
