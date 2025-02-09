using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpPcap;

namespace FHAPI.Core
{
    public abstract class BaseComponent
    {
        protected BaseComponent(ref ConcurrentQueue<RawCapture> packetsQueue)
        {
            _packetsQueue = packetsQueue;
        }
        protected readonly ConcurrentQueue<RawCapture> _packetsQueue;
        public CaptureDeviceList CaptureDevices => CaptureDeviceList.Instance;
    }
}
