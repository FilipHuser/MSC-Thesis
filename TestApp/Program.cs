namespace TestApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            FHAPILib.FHAPI fhapi = new FHAPILib.FHAPI();
            fhapi.Run();

            while (true) 
            { 
                Console.WriteLine($"CAPTURED PACKETS QUEUE: {fhapi.PacketsCount}");
                Console.WriteLine($"BUFFER QUEUE: {fhapi._processor.BufferSize}");
                Thread.Sleep(100);
            }
        } 
    }
} 