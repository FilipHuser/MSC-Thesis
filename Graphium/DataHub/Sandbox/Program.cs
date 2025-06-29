using System;
using System.Linq;
using System.Threading;
using DataHub.Modules;
using DataHub;
using SharpPcap;

namespace Sandbox
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var hub = new Hub();
            var packetModule = new PacketModule<RawCapture>(3, "udp and src 192.168.50.245");
            var httpModule = new HTTPModule<string>("http://localhost:8888/");

            hub.AddModule(packetModule);
            hub.AddModule(httpModule);

            hub.StartCapturing();

            Console.WriteLine("Press ENTER to stop...\n");

            while (true)
            {
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(intercept: true);
                    if (key.Key == ConsoleKey.Enter)
                        break;
                }

                var packetData = hub.Modules[packetModule.GetType()].Get<RawCapture>(x => true).ToList();
                var httpData = hub.Modules[httpModule.GetType()].Get<string>(x => true).ToList();

                Console.SetCursorPosition(0, 0);
                Console.Write($"Packet size: {packetData.Count()}, HTTP size: {httpData.Count()}    ");

                Thread.Sleep(1000);
            }
            hub.StopCapturing();
        }
    }
}
