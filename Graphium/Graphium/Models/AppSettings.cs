namespace Graphium.Models
{
    internal class AppSettings : ModelBase
    {
        public int PayloadSize { get; set; } = 18;
        public int CaptureDeviceIndex { get; set; } = 3;
        public string IPAddr { get; set; } = "192.168.50.245";
        public string URI { get; set; } = "http://localhost:8888/";
        public string AppName { get; set; } = "Graphium";
        public string Version { get; set; } = "1.0.0";
    }
}
