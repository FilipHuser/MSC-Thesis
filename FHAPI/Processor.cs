using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using FHAPI.Core;
using PacketDotNet;
using PacketDotNet.Ieee80211;
using SharpPcap;
using Spectre.Console;

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
        private ConcurrentQueue<RawCapture> _packetBuffer { get; set; } = new ConcurrentQueue<RawCapture>();
        public int BufferSize => _packetBuffer.Count;
        private Thread? _bufferThread { get; set; }
        private CancellationTokenSource _cancellationTokenSource { get; set; } = new CancellationTokenSource();
        #endregion

        public Processor(ref ConcurrentQueue<RawCapture> packetsQueue) : base(ref packetsQueue)
        {
            PacketBatchSize = 1;
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
        public List<FHPacket> GetPackets(Func<int , bool> filterFunction)
        {
            var packets = new List<FHPacket>();
            var counter = 0;
            while (packets.Count < PacketBatchSize && !_packetBuffer.IsEmpty)
            {
                if (_packetBuffer.TryDequeue(out RawCapture? packet))
                {
                    if (filterFunction(counter)) { packets.Add(new FHPacket(packet)); }
                }
                counter++;
            }
            return packets;
        }
        #endregion
    }
}
