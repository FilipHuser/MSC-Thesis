using System.Diagnostics;
using System.Net.Mime;
using FHAPI.Core;
using FHAPILib;
using FHMonitor.Models.ViewModels;
using FHMonitor.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SharpPcap;

namespace FHMonitor.Controllers
{
    public class MonitoringController : Controller
    {
        #region SERVICES
        private readonly ILogger<HomeController> _logger;
        private readonly FHAPIService _fhapis;
        #endregion
        public MonitoringController(ILogger<HomeController> logger , FHAPIService fhapis)
        {
            _logger = logger;
            _fhapis = fhapis;
        }
        public IActionResult Index()
        {
            ViewBag.captureDevices = _fhapis.GetCaptureDevices();
            return View(new MonitorSettingsViewModel());
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult StartCapturing(MonitorSettingsViewModel ms)
        {
            if (!ModelState.IsValid) { return Error(); }
            _fhapis.StartCapturing(ms);
            return View("Monitor", ms);
        }
        public IActionResult StopCapturing()
        {
            _fhapis.StopCapturing();
            return RedirectToAction("Index");
        }
        [HttpGet]
        public IActionResult GetPackets()
        {
            var packets = _fhapis.GetPackets()
                .Select(x => {
                    return new
                    {
                        Timestamp = x.Timestamp,
                        Value = Convertor<short>.ConvertPayload(x.Payload.Skip(2).Take(2).ToArray(), 0)
                    };
                }).ToList();

            return Json(packets);
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
