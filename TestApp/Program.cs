using FHAPILib;
using PacketDotNet;
using PacketDotNet.Ieee80211;
using SharpPcap;

namespace TestApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            FHAPI fhapi = new FHAPI();
            fhapi.Filter = "udp and src host 192.168.50.245";

            foreach (var dev in CaptureDeviceList.Instance)
            {
                Console.WriteLine(dev.Description);
            }


            fhapi.StartCapturing(3);
            while (true)
            {
                RawCapture? rawPacket;
                if (fhapi.CapturedPackets.TryDequeue(out rawPacket))
                {
                    // Parse the raw packet to an EthernetPacket
                    var packet = Packet.ParsePacket(rawPacket.LinkLayerType, rawPacket.Data);

                    // Check if the packet is an IP packet
                    if (packet is EthernetPacket ethPacket)
                    {
                        // Further parse for IP layer (IPv4)
                        if (ethPacket.PayloadPacket is IPv4Packet ipPacket)
                        {
                            // Check if the packet is a UDP packet
                            if (ipPacket.PayloadPacket is UdpPacket udpPacket)
                            {
                                // Extract details of the UDP packet
                                Console.WriteLine("UDP Packet:");
                                Console.WriteLine($"Source IP: {ipPacket.SourceAddress}");
                                Console.WriteLine($"Destination IP: {ipPacket.DestinationAddress}");
                                Console.WriteLine($"Source Port: {udpPacket.SourcePort}");
                                Console.WriteLine($"Destination Port: {udpPacket.DestinationPort}");
                                Console.WriteLine($"UDP Length: {udpPacket.PayloadData.Length}");
                                Console.WriteLine($"Data: {BitConverter.ToString(udpPacket.PayloadData)}");

                                // If you want to interpret the payload as a string (assuming it is ASCII or UTF-8 encoded)
                                string payloadText = System.Text.Encoding.UTF8.GetString(udpPacket.PayloadData);
                                Console.WriteLine($"Payload Text: {payloadText}");
                            }
                        }
                    }
                }
            }

            fhapi.StopCapturing();
        } 
    }
} 