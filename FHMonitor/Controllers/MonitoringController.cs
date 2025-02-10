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
        [HttpPost]
        public IActionResult SetSettings([FromBody] MonitorSettingsViewModel ms)
        {
            _fhapis.SetSetting(ms);
            return Ok();
        }
    }
}
