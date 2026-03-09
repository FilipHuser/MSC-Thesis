using System.Net;
using System.Net.Sockets;
using System.Text;

namespace DataHub.Core
{
    public class UdpExporter : IDisposable
    {
        private UdpClient? _client;
        private readonly IPEndPoint _endpoint;
        private bool _disposed;

        public UdpExporter(string host, int port)
        {
            _endpoint = new IPEndPoint(IPAddress.Parse(host), port);
            _client = new UdpClient();
        }
        public void Send(string json)
        {
            try
            {
                var bytes = Encoding.UTF8.GetBytes(json);
                _client.Send(bytes, bytes.Length, _endpoint);
                System.Diagnostics.Debug.WriteLine($"UDP sent {bytes.Length} bytes to {_endpoint}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UDP send error: {ex.Message}");
            }
        }

        public void Restart()
        {
            _client?.Dispose();
            _client = new UdpClient();
            _disposed = false;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _client?.Dispose();
                _client = null;
                _disposed = true;
            }
        }
    }
}