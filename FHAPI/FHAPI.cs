using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using PacketDotNet;
using SharpPcap;
using System.Net.Sockets;

namespace FHAPILib
{
    public class FHAPI : IDisposable
    {
        public int CapturedPacketsCount => _capturedPackets.Count;
        public int BufferedPacketsCount => _processor.BufferSize;

        private readonly Capturer _capturer;
        public readonly Processor _processor;
        private ConcurrentQueue<RawCapture> _capturedPackets = new ConcurrentQueue<RawCapture>();
        private ConcurrentQueue<RawCapture> CapturedPackets
        {
            get => _capturedPackets;
            set { _capturedPackets = value;  }  
        }
        public int PacketsCount => _capturedPackets.Count;
        public FHAPI()
        {
            _processor = new Processor(ref _capturedPackets);
            _capturer = new Capturer(ref _capturedPackets);
            _capturer.OnStartCapturing += (sender , e) => _processor.StartBuffering();
            _capturer.OnStopCapturing += (sender, e) => _processor.StopBuffering();
        }
        public void Run()
        {
            _capturer.StartCapturing();
        }

        public void Dispose()
        {
            _capturer.StopCapturing();
        }
    }
}
