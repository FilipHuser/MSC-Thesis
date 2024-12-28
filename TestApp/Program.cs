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
            //fhapi.Filter = "udp and src host 192.168.2.6";

            DateTime startTime = DateTime.Now;
            TimeSpan timeLimit = TimeSpan.FromSeconds(5);


            fhapi.StartCapturing(4);
            while (true)
            {
                if (DateTime.Now - startTime > timeLimit)
                {
                    Console.WriteLine("Time limit reached. Stopping capture.");
                    break;
                }

                RawCapture? packet;
                if (fhapi.CapturedPackets.TryDequeue(out packet))
                {
                    var p = Packet.ParsePacket(packet.LinkLayerType, packet.Data);

                    if (p.PayloadPacket is IPv4Packet ipPacket)
                    {
                        Console.WriteLine($"dst: {ipPacket.DestinationAddress} | src: {ipPacket.SourceAddress}");
                    }
                }
            }

            fhapi.StopCapturing();
        } 
    }
} 