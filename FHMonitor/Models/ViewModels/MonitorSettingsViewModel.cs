using System.ComponentModel.DataAnnotations;

namespace FHMonitor.Models.ViewModels
{
    public class MonitorSettingsViewModel
    {
        #region PROPERTIES
        [Display(Name = "Capture Device")]

        [Required(ErrorMessage = "Device is required.")]
        public int CaptureDeviceIndex { get; set; }
        public string? Filter { get; set; }
        #endregion
    }
}
