using System.Net;
using System.Net.Sockets;
using System.Text;
using GraphiumLive.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace GraphiumLive.Services
{
    public class UdpReceiverService : BackgroundService
    {
        private readonly IHubContext<LiveDataHub> _hubContext;
        private readonly ILogger<UdpReceiverService> _logger;
        private readonly int _port;

        public UdpReceiverService(IHubContext<LiveDataHub> hubContext, ILogger<UdpReceiverService> logger, IConfiguration configuration)
        {
            _hubContext = hubContext;
            _logger = logger;
            _port = configuration.GetValue<int>("UdpPort", 9998);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var udpClient = new UdpClient(_port);
            _logger.LogInformation("UDP Receiver listening on port {Port}", _port);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var result = await udpClient.ReceiveAsync(stoppingToken);
                    var json = Encoding.UTF8.GetString(result.Buffer);
                    await _hubContext.Clients.All.SendAsync("ReceiveData", json, stoppingToken);
                }
                catch (OperationCanceledException) { break; }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "UDP receive error");
                }
            }
        }
    }
}