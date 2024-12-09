using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace FHAPI
{
    public enum ChannelType
    {
        ANALOG,
        DIGITAL,
        CALC,
        FACE_READER
    }
    public class AcqNdtChannel
    {
        public AcqNdtChannel(ChannelType channelType , int index = 0 , int dataSize = 8 , int samplingDivider = 1,
                             float scale = 1.0f , float offset = 0.0f , bool enabledForDelivery = true)
        {
            ChannelType = channelType;
            Index = index;
            DataSize = dataSize;
            SamplingDivider = samplingDivider;
            Scale = scale;
            Offset = offset;
            EnabledForDelivery = enabledForDelivery;
        }

        #region PROPERTIES
        public ChannelType ChannelType { get; set; }
        public int Index { get; set; }
        public int DataSize { get; set; }
        public int SamplingDivider { get; set; }
        public float Scale { get; set; }
        public float Offset { get; set; }
        public bool EnabledForDelivery { get; set; }
        #endregion

        #region METHODS
        public (ChannelType, int) GetSimpleChannelStruct() => (ChannelType, Index);
        #endregion
        public override string ToString()
        {
            return string.Join("\n", this.GetType().GetProperties().Select(x => $"{x.Name}: {x.GetValue(this)}"));
        }
    }
}
