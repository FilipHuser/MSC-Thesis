using FHAPILib;
using FHMonitor.Services;
using Microsoft.AspNetCore.Mvc;

namespace FHMonitor.Controllers
{
    public class MonitoringController : Controller
    {
        #region SERVICES
        private readonly FHAPIService _fhapis;
        #endregion
        public MonitoringController(FHAPIService fhapis)
        {
            _fhapis = fhapis;
        }
        public IActionResult Index()
        {
            ViewBag.captureDevices = _fhapis.GetCaptureDevices();
            return View();
        }
    }
}
