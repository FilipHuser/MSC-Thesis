using PCAPILib;

namespace Snadbox
{
    internal class Program
    {
        static void Main(string[] args)
        {
            PCAPI api = new PCAPI();


            CancellationTokenSource cts = new CancellationTokenSource();

            api.SetDeviceIndex(3);
            api.SetFilter("udp and src host 192.168.50.245");
            api.StartCapturing();

            api.Monitor(cts.Token);
        }
    }
}
