using Spectre.Console;

namespace TestApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var cts = new CancellationTokenSource();
            using (FHAPILib.FHAPI fhapi = new FHAPILib.FHAPI())
            {
                fhapi.Run();

                Task.Run(() => fhapi.Monitor(cts.Token));

                Console.ReadKey();
                cts.Cancel();
            }
        } 
    }
} 