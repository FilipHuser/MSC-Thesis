using FHAPI;

namespace TestAppn
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var anc = new AcqNdtChannel(ChannelType.DIGITAL);

            Console.WriteLine(anc);
        }
    }
}
