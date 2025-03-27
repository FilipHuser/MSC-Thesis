using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
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
            set
            {
                if (value > 0) { _packetBatchSize = value; }
                else { throw new ArgumentException("Invalid batch size!"); }
            }
        }

        private readonly ConcurrentQueue<RawCapture> _packetBuffer = new();
        public int BufferSize => _packetBuffer.Count;

        private Thread? _bufferThread;
        private CancellationTokenSource _cancellationTokenSource = new();
        private readonly object _lock = new();  // Lock object for thread safety
        #endregion

        public Processor(ref ConcurrentQueue<RawCapture> packetsQueue) : base(ref packetsQueue)
        {
            PacketBatchSize = 1000;
        }

        #region METHODS
        public void StartBuffering()
        {
            lock (_lock)
            {
                if (_bufferThread is { IsAlive: true }) return;

                _cancellationTokenSource = new CancellationTokenSource();
                var cancellationToken = _cancellationTokenSource.Token;

                _bufferThread = new Thread(() =>
                {
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        while (_packetsQueue.TryDequeue(out RawCapture? packet))
                        {
                            _packetBuffer.Enqueue(packet);
                        }
                        Thread.Sleep(1);
                    }
                })
                {
                    IsBackground = true
                };
                _bufferThread.Start();
            }
        }

        public void StopBuffering()
        {
            lock (_lock)
            {
                _cancellationTokenSource?.Cancel();
                _bufferThread?.Join();
                _cancellationTokenSource?.Dispose();
            }
        }

        public List<FHPacket> GetPackets(Func<int, bool> filterFunction)
        {
            var packets = new List<FHPacket>();
            var counter = 0;

            while (packets.Count < PacketBatchSize && !_packetBuffer.IsEmpty)
            {
                if (_packetBuffer.TryDequeue(out RawCapture? packet))
                {
                    if (filterFunction(counter))
                    {
                        packets.Add(new FHPacket(packet));
                    }
                    counter++;
                }
            }
            return packets;
        }
        #endregion
    }
}
