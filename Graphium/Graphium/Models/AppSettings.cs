namespace Graphium.Models
{
    internal class AppSettings : ModelBase
    {
        private int _payloadSize = 18;
        public int PayloadSize
        {
            get => _payloadSize;
            set => SetProperty(ref _payloadSize, value);
        }

        private int _captureDeviceIndex = 3;
        public int CaptureDeviceIndex
        {
            get => _captureDeviceIndex;
            set => SetProperty(ref _captureDeviceIndex, value);
        }

        private string _ipAddr = "192.168.50.245";
        public string IPAddr
        {
            get => _ipAddr;
            set => SetProperty(ref _ipAddr, value);
        }

        private string _uri = "http://localhost:8888/";
        public string URI
        {
            get => _uri;
            set => SetProperty(ref _uri, value);
        }
    }
}