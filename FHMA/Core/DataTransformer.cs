using System.Dynamic;
using System.Text.Json;
using FHMA.Models.ExternalModels;
using static FHAPI.Core.FHPacket;

namespace FHMA.Core
{
    internal static class DataTransformer
    {
        public static object Transform(List<(DateTime , object)> data , PacketSource? source)
        {
            var output = new object();
            switch (source)
            {
                case PacketSource.STATION:

                    var chars = data.Select(x => Convert.ToChar(x.Item2));
                    var json = $"{{{string.Concat(chars)}}}";
                    output = JsonSerializer.Deserialize<KMModel>(json);
                    break;

                case PacketSource.BIOPAC:
                case PacketSource.EMOTIV:
                    output = data.Last().Item2;
                    break;
            }

            return output;
        }
    }
}
