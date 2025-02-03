namespace TestApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            using (FHAPILib.FHAPI fhapi = new FHAPILib.FHAPI())
            {
                fhapi.Run();


                Console.WriteLine($"CAPTURED PACKETS QUEUE: {fhapi.CapturedPacketsCount}");
                Console.WriteLine($"BUFFER QUEUE: {fhapi.BufferedPacketsCount}");

                Thread.Sleep(1000);

                Console.WriteLine($"CAPTURED PACKETS QUEUE: {fhapi.CapturedPacketsCount}");
                Console.WriteLine($"BUFFER QUEUE: {fhapi.BufferedPacketsCount}");
            }
        } 
    }
} 