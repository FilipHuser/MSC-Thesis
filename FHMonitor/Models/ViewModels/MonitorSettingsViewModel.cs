using System.ComponentModel.DataAnnotations;
using SharpPcap;

namespace FHMonitor.Models.ViewModels
{
    public class MonitorSettingsViewModel
    {
        #region PROPERTIES
        [Display(Name = "Capture Device")]
        [Required(ErrorMessage = "Capture device is required.")]
        public string? CaptureDeviceName { get; set; }
        public string? Filter { get; set; } = "src host 192.168.50.245 and udp and len >= 28";
        #endregion
    }
}
