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
            PacketBatchSize = 1;
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
                        //Thread.Sleep(500);
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
        public List<RawCapture> GetPackets(QueueProcessorFunc? processorFunc = null)
        {
            var batch = new List<RawCapture>();
            for (int i = 0; i < PacketBatchSize; i++)
            {
                if (_packetBuffer.TryDequeue(out RawCapture? packet)) { batch.Add(packet); }
            }
            return batch;
        }
        #endregion
    }
}
