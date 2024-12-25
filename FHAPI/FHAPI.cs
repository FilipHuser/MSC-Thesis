using System;
using System.Collections.Generic;
using PacketDotNet;
using SharpPcap;

namespace FHAPILib
{
    public class FHAPI
    {
        public FHAPI()
        {
            ReadTimeout = 1000;
            Filter = "udp";
        }

        #region PROPERTIES
        public int ReadTimeout { get; set; } //ms
        public string Filter { get; set; }
        #endregion

        #region METHODS
        public void Run()
        {
            ListCaptureDevices();
            Console.WriteLine();
            Console.Write("-- Please choose a device to capture: ");
            if (!int.TryParse(Console.ReadLine(), out int deviceIndex)) { return; }
            Capture(deviceIndex);
            Console.WriteLine("\nPress Enter to stop capturing...\n");
            Console.ReadLine();
        }

        private void ListCaptureDevices()
        {
            Console.WriteLine("The following devices are available on this machine:");
            Console.WriteLine("----------------------------------------------------");
            Console.WriteLine();
            int i = 0;
            foreach (var dev in CaptureDeviceList.Instance)
            {
                Console.WriteLine("{0}) {1}", i, dev.Description);
                i++;
            }
        }

        private async void Capture(int deviceIndex)
        {
            var devices = CaptureDeviceList.Instance;
            using var device = devices[deviceIndex];

            device.OnPacketArrival += new PacketArrivalEventHandler(device_OnPacketArrival);
            device.Open(DeviceModes.Promiscuous, ReadTimeout);
            device.Filter = Filter;

       
            await Task.Run(() => { device.Capture(); });
            
            device.StartCapture();
            device.Close();
        }



        private static void device_OnPacketArrival(object sender, PacketCapture e)
        {
            Console.WriteLine($"Len={e.Data.Length}:\n {BitConverter.ToString(e.Data.ToArray()).Replace("-", " ")}\n\n");
        }
        #endregion
    }
}
