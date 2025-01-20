using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using FHAPI.Core;
using SharpPcap;

namespace FHAPILib
{
    public class Processor : BaseComponent
    {
        private int _packetBatchSize; 
        public int PacketBatchSize 
        {
            get => _packetBatchSize;
            set { if (value > 0) { _packetBatchSize = value; } else { throw new ArgumentException("Invalid batch size !"); } }
        }
        private Queue<RawCapture> PacketBuffer = new Queue<RawCapture>();
        private Thread? _bufferThread { get; set; }
        public Processor(ref ConcurrentQueue<RawCapture> packetsQueue) : base(ref packetsQueue)
        {
            PacketBatchSize = 1;
        }
        public delegate void QueueProcessorFunc();
        #region METHODS



        public List<RawCapture> GetPackets()
        {
            var batch = new List<RawCapture>();
            for (int i = 0; i < PacketBatchSize; i++)
            {
                if (_packetsQueue.TryDequeue(out RawCapture? packet)) { batch.Add(packet); }
            }
            return batch;
        }
        #endregion
    }
}
