using System.ComponentModel.DataAnnotations;

namespace FHMonitor.Models.ViewModels
{
    public class MonitorSettingsViewModel
    {
        #region PROPERTIES
        [Display(Name = "Capture Device")]
        public int CaptureDeviceIndex { get; set; }
        public string? Filter { get; set; }
        public int ReadTimeout { get; set; } = 100;
        public int NPacketsPerFetch { get; set; }
        #endregion
    }
}
