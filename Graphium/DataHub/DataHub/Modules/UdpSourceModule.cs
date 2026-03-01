using System.Net;
using System.Net.Sockets;
using DataHub.Core;

namespace DataHub.Modules
{
    public class UdpSourceModule : ModuleBase<byte[]>
    {
        #region PROPERTIES
        private readonly int _port;
        private UdpClient? _udpClient;
        public override double SamplingRate => 0.98;
        public override ModuleType ModuleType => ModuleType.UDP;
        #endregion
        #region METHODS
        public UdpSourceModule(int port)
        {
            _port = port;
        }
        protected override async Task CaptureTask(CancellationToken ct)
        {
            _udpClient?.Dispose();
            _udpClient = new UdpClient(_port);
            try
            {
                while (!ct.IsCancellationRequested)
                {
                    var result = await _udpClient.ReceiveAsync(ct);
                    Enqueue(new CapturedData<byte[]>(DateTime.Now, result.Buffer, this));
                }
            }
            catch (OperationCanceledException) { }
            finally
            {
                _udpClient?.Dispose();
                _udpClient = null;
            }
        }
        public override void Dispose()
        {
            StopCapturing();
            _udpClient?.Dispose();
            _udpClient = null;
        }
        #endregion
    }
}