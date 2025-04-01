using FHMA.Models;
using static FHAPI.Core.FHPacket;

namespace FHMA.Models
{
    public class BiometricSignal
    {
        public string? Name { get; set; }
        public PacketSource Source { get; set; }
        public List<Graph> Graphs { get; set; } = new List<Graph>();

        #region METHODS
        public override string ToString()
        {
            return $"{Name} ({Source}) [{Graphs.Count}CH]";
        }
        #endregion
    }
}
