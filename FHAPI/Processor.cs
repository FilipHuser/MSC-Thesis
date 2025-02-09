using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using FHAPI.Core;
using PacketDotNet;
using SharpPcap;

namespace FHAPILib
{
    public class Processor : BaseComponent
    {
        #region PROPERTIES
        private int _packetBatchSize;
        public int PacketBatchSize
        {
            get => _packetBatchSize;
            set { if (value > 0) { _packetBatchSize = value; } else { throw new ArgumentException("Invalid batch size !"); } }
        }
        public bool IsBufferingActive { get; set; } = false;
        private readonly Mutex _IBAMutex;
        private ConcurrentQueue<RawCapture> _packetBuffer { get; set; } = new ConcurrentQueue<RawCapture>();
        public int BufferSize => _packetBuffer.Count;
        private Thread? _bufferThread { get; set; }
        private CancellationTokenSource _cancellationTokenSource { get; set; } = new CancellationTokenSource();
        public delegate void QueueProcessorFunc();
        #endregion
        
        public Processor(ref ConcurrentQueue<RawCapture> packetsQueue) : base(ref packetsQueue)
        {
            PacketBatchSize = 100;
            _IBAMutex = new Mutex();
        }

        #region METHODS
        public void StartBuffering()
        {
            var cancellationToken = _cancellationTokenSource.Token;
            _bufferThread = new Thread(() => 
            {

                while (!cancellationToken.IsCancellationRequested)
                {
                    while (_packetsQueue.TryDequeue(out RawCapture? packet))
                    {
                        _packetBuffer.Enqueue(packet);
                    }
                }
            });
            _bufferThread.Start();
        }
        public void StopBuffering()
        {
            _cancellationTokenSource?.Cancel();
            _bufferThread?.Join();
        }
        public List<IPv4Packet> GetPackets(QueueProcessorFunc? processorFunc = null)
        {
            var batch = new List<IPv4Packet>();
            for (int i = 0; i < PacketBatchSize; i++)
            {
                if (_packetBuffer.TryDequeue(out RawCapture? packet))
                {
                    if (Packet.ParsePacket(packet.LinkLayerType, packet.Data) is IPv4Packet ipPacket)
                    {
                        batch.Add(ipPacket);
                    }
                }
            }
            return batch;
        }
        #endregion
    }
}
