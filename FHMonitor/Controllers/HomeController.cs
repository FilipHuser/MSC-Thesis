using System.Diagnostics;
using FHMonitor.Models;
using Microsoft.AspNetCore.Mvc;
using FHAPILib;
using FHMonitor.Services;
using PacketDotNet;

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

        public IActionResult _FetchPacketsPartial()
        {
            var packets = _fhapis.GetPackets(5);
            ViewBag.N = _fhapis.PacketCount;
            return PartialView(packets);
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
