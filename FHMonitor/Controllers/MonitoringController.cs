using System.Net.Mime;
using FHAPI.Core;
using FHAPILib;
using FHMonitor.Models.ViewModels;
using FHMonitor.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

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
            ViewBag.captureDevices = _fhapis.GetCaptureDevices().Select((x, i) => new { Name = x.Description , Value = i });
            return View();
        }
        public IActionResult StartCapturing()
        {
            _fhapis.StartCapturing();
            return RedirectToAction("Index");
        }
        public IActionResult StopCapturing()
        {
            _fhapis.StartCapturing();
            return RedirectToAction("Index");
        }
        [HttpGet]
        public IActionResult GetPackets()
        {
            var packets = _fhapis.GetPackets().Select(x => FHAPILib.Convertor<int>.ConvertPayload(x.Payload , 0)).ToList();
            return Json(packets);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SetSettings([FromBody] MonitorSettingsViewModel ms)
         {
            if (!ModelState.IsValid) { return StatusCode(500); }

            _fhapis.SetSetting(ms);
            return Ok();
        }
    }
}
