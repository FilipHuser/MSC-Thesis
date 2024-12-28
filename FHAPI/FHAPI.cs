using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using PacketDotNet;
using SharpPcap;

namespace FHAPILib
{
    public class FHAPI
    {
        public FHAPI()
        {
            ReadTimeout = 1000;
        }

        #region PROPERTIES
        public CaptureDeviceList CaptureDevices => CaptureDeviceList.Instance;
        public int ReadTimeout { get; set; } //ms
        public string? Filter { get; set; }
        public ConcurrentQueue<RawCapture> CapturedPackets { get; set; } = new ConcurrentQueue<RawCapture>();
        private ILiveDevice CaptureDevice { get; set; }
        private Thread CaptureThread { get; set; }
        #endregion
         
        #region METHODS
        public void StartCapturing(int deviceIndex)
        {
            var devices = CaptureDeviceList.Instance;
            CaptureDevice = devices[deviceIndex];

            CaptureDevice.OnPacketArrival += new PacketArrivalEventHandler(device_OnPacketArrival);
            CaptureDevice.Open(DeviceModes.Promiscuous, ReadTimeout);
            CaptureDevice.Filter = Filter??"";

            CaptureThread = new Thread(() =>
            {
                CaptureDevice.StartCapture();
            });

            CaptureThread.Start();
        }
        public void StopCapturing()
        {
            if (CaptureDevice == null) { return; }
            CaptureDevice.StopCapture();
            CaptureDevice.Close();

            if (CaptureThread != null && CaptureThread.IsAlive)
            {
                CaptureThread.Join();
            }
        }

        private void device_OnPacketArrival(object sender, PacketCapture e)
        {
            CapturedPackets.Enqueue(e.GetPacket());
        } 
        #endregion
    }
}
