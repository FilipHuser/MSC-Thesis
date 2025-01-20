using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FHAPI.Core;
using SharpPcap;

namespace FHAPILib
{
    public class Capturer : BaseComponent
    {
        protected Capturer(ref ConcurrentQueue<RawCapture> capturedPackets) : base(ref capturedPackets) { }
        #region PROPERTIES
        protected string? Filter { get; set; }
        private int _readTimeout { get; set; } = 1000; //ms
        protected int ReadTimeout
        {
            get => _readTimeout;
            set => _readTimeout = (value > 0) ? value : throw new ArgumentOutOfRangeException(nameof(value), "ReadTimeout must be greater than zero.");
        }
        protected bool IsCapturing { get; set; } = false;
        private ILiveDevice? _captureDevice { get; set; }
        private Thread? _captureThread { get; set; }
        #endregion
        #region METHODS
        protected void StartCapturing(int deviceIndex)
        {
            var devices = CaptureDevices;
            _captureDevice = devices[deviceIndex];

            _captureDevice.OnPacketArrival += new PacketArrivalEventHandler(device_OnPacketArrival);
            _captureDevice.Open(DeviceModes.Promiscuous, ReadTimeout);
            _captureDevice.Filter = Filter ?? "";

            _captureThread = new Thread(() =>
            {
                _captureDevice.StartCapture();
            });

            _captureThread.Start();
        }
        protected void StopCapturing()
        {
            if (_captureDevice == null) { return; }
            _captureDevice.StopCapture();
            _captureDevice.Close();

            if (_captureThread != null && _captureThread.IsAlive)
            {
                _captureThread.Join();
            }
        }
        protected void device_OnPacketArrival(object sender, PacketCapture e)
        {
            _packetsQueue.Enqueue(e.GetPacket());
        }
        #endregion
    }
}
