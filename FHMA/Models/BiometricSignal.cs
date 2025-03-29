using FHMA.Models;
using static FHAPI.Core.FHPacket;

namespace FHMA.Models
{
    public enum SignalType
    {
        AN,     // Analog
        EKG,    // Electrocardiogram
        EEG,    // Electroencephalogram
        RESP,   // Respiration
        //HR,     // Heart Rate
        //SPO2,   // Oxygen Saturation
        //EDA,    // Electrodermal Activity
    }
    public class BiometricSignal
    {
        public SignalType Type { get; set; }
        public PacketSource Source { get; set; }
        public List<Graph> Graphs { get; set; } = new List<Graph>();

        #region METHODS
        public void UpdateData(Dictionary<int, List<(DateTime, double)>> points)
        {
        }
        public override string ToString()
        {
            return $"{Type} ({Source}) [{Graphs.Count}CH]";
        }
        #endregion
    }
}
