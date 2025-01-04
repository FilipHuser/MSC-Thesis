namespace FHMonitor.Models
{
    public class MonitorSettingsViewModel
    {
        #region PROPERTIES
        public int CaptureDeviceIndex { get; set; }
        public string? Filter { get; set; }
        public int ReadTimeout { get; set; }
        public int NPacketsPerFetch { get; set; }
        #endregion
    }
}
