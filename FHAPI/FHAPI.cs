using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using PacketDotNet;
using SharpPcap;
using System.Net.Sockets;

namespace FHAPILib
{
    public class FHAPI
    {
        private readonly Processor _processor;
        private ConcurrentQueue<RawCapture> _capturedPackets = new ConcurrentQueue<RawCapture>();
        private ConcurrentQueue<RawCapture> CapturedPackets
        {
            get => _capturedPackets;
            set { _capturedPackets = value;  }  
        }
        public FHAPI()
        {
            _processor = new Processor(ref _capturedPackets);
        }
    }
}
