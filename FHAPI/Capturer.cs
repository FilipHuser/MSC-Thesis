using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using FHAPI.Core;
using SharpPcap;
using Spectre.Console;

namespace FHAPILib
{
    public class Capturer : BaseComponent
    {
        #region PROPERTIES
        private int _deviceIndex { get; set; }
        public int DeviceIndex 
        {
            get => _deviceIndex;
            set
            {
                if (value < 0) { throw new ArgumentOutOfRangeException(nameof(value), $"Device Index must be in range of <0 , {CaptureDevices.Count})"); }
                _deviceIndex = value;
            }
        }
        public string? Filter { get; set; }
        private int _readTimeout { get; set; } = 10; //ms
        protected int ReadTimeout
        {
            get => _readTimeout;
            set => _readTimeout = (value > 0) ? value : throw new ArgumentOutOfRangeException(nameof(value), "ReadTimeout must be greater than zero.");
        }
        private ILiveDevice? _captureDevice { get; set; }
        public ILiveDevice? CaptureDevice
        {
            get => DeviceIndex >= 0 && DeviceIndex < CaptureDevices.Count ? CaptureDevices [DeviceIndex] : null;
        }
        private Thread? _captureThread { get; set; }
        
        public event EventHandler? OnStartCapturing;
        public event EventHandler? OnStopCapturing;
        #endregion
        public Capturer(ref ConcurrentQueue<RawCapture> capturedPackets) : base(ref capturedPackets) { }
        #region METHODS
        public void StartCapturing()
        {
            var devices = CaptureDevices;
            _captureDevice = devices[DeviceIndex];
            _captureDevice.OnPacketArrival += new PacketArrivalEventHandler(device_OnPacketArrival);
            _captureDevice.Open(DeviceModes.Promiscuous, ReadTimeout);
            _captureDevice.Filter = Filter ?? "";
            _captureThread = new Thread(() => { _captureDevice.StartCapture(); });
            OnStartCapturing?.Invoke(this, EventArgs.Empty);
            _captureThread.Start();
        }
        public void StopCapturing()
        {
            _captureDevice?.StopCapture();
            _captureDevice?.Close();
            _captureThread?.Join();
            OnStopCapturing?.Invoke(this, EventArgs.Empty);
        }
        protected void device_OnPacketArrival(object sender, PacketCapture e)
        {
            _packetsQueue.Enqueue(e.GetPacket());
        }
        #endregion
    }
}
