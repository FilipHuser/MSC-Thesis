using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)  // Mark Main as async Task
    {
        FHAPILib.FHAPI fhapi = new FHAPILib.FHAPI();
        fhapi.SetDeviceIndex(4);
        fhapi.SetFilter("");
        fhapi.StartCapturing();


        CancellationTokenSource cts = new CancellationTokenSource();

        await Task.Run(() => fhapi.Monitor(cts.Token));

        while (true) ;
    }
}
