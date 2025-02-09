using Spectre.Console;

namespace TestApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            AnsiConsole.Write(new FigletText("FHAPI").Centered().Color(Color.Blue));

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