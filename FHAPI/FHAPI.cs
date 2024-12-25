using System.Threading;
using PacketDotNet;
using SharpPcap;
using SharpPcap.LibPcap;

namespace FHAPILib
{
    public class FHAPI
    {
        #region METHODS
        public void L()
        {
            var devices = CaptureDeviceList.Instance;

            foreach (var dev in devices)
            {
                Console.WriteLine(dev);
            }
        }

        public async Task Capture(int deviceIndex , CancellationToken cancellationToken)
        {
            if (deviceIndex >= LibPcapLiveDeviceList.Instance.Count || deviceIndex < 0) throw new Exception("Invalid device index!");

            using var device = LibPcapLiveDeviceList.Instance[deviceIndex];

            device.Open();
            device.Filter = "udp";

            device.OnPacketArrival += Device_OnPacketArrival;

            await Task.Run(() =>
            {
                device.StartCapture();

                // Capture packets until cancellation is requested
                Console.WriteLine("Capturing packets... Press Ctrl+C to stop.");
                while (!cancellationToken.IsCancellationRequested)
                {
                }

                device.StopCapture();
                device.Close();
            });
        }
        private void Device_OnPacketArrival(object s, PacketCapture e)
        {
            Console.WriteLine(BitConverter.ToString(e.Data.ToArray()).Replace("-", " "));

        }
        #endregion

    }
}
