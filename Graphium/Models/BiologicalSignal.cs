using Graphium.Models;
using static PCAPILib.Core.PCAPIPacket;

namespace Graphium.Models
{
    public class BiologicalSignal
    {
        public string? Name { get; set; }
        public PacketSource Source { get; set; }
        public List<Graph> Graphs { get; set; } = new List<Graph>();

        #region METHODS
        public override string ToString() => $"{Name} ({Source}) [{Graphs.Count}CH]";
        #endregion
    }
}
