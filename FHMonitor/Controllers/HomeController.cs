using System.Diagnostics;
using FHMonitor.Models;
using Microsoft.AspNetCore.Mvc;
using FHAPILib;
using FHMonitor.Services;
using PacketDotNet;
using Microsoft.Extensions.Logging;

namespace FHMonitor.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly FHAPIService _fhapis;

        public HomeController(ILogger<HomeController> logger , FHAPIService fhapis)
        {
            _logger = logger;
            _fhapis = fhapis;
        }

        public IActionResult Index()
        {
            FHAPI fhapi = new FHAPI();

            ViewBag.devices = fhapi.CaptureDevices;
            return View();
        }
        public IActionResult CapturePackets(MonitorSettingsViewModel ms)
        {
            _fhapis.NPacketsPerFetch = ms.NPacketsPerFetch;
            _fhapis.FHAPI.Filter = ms.Filter;
            _fhapis.StartCapturing(ms.CaptureDeviceIndex);

            return RedirectToAction("Index");
        }
        public IActionResult _FetchPacketsPartial()
        {
            var packets = _fhapis.GetPackets();

            var response = new List<int>();

            //_logger.LogInformation(_fhapis.FHAPI.CapturedPackets.Count.ToString());

            foreach (var packet in packets)
            {
                if (Packet.ParsePacket(packet.LinkLayerType, packet.Data) is EthernetPacket { PayloadPacket: IPv4Packet { PayloadPacket: UdpPacket udpPacket } })
                {
                    //_logger.LogInformation(BitConverter.ToInt16(udpPacket.PayloadData).ToString());
                    var val = BitConverter.ToInt16(udpPacket.PayloadData);

                    if (val != 1) { response.Add(BitConverter.ToInt16(udpPacket.PayloadData)); }
                }
            }
            return Json(response);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
