namespace Graphium.Models
{
    internal class AppSettings : ModelBase
    {
        #region PCAP_MODULE
        private int _payloadSize = 18;
        public int PayloadSize
        {
            get => _payloadSize;
            set => SetProperty(ref _payloadSize, value);
        }
        private int _captureDeviceIndex = 0;
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
        #endregion
        #region HTTP_MODULE
        private string _uri = "http://*:8888/";
        public string URI
        {
            get => _uri;
            set => SetProperty(ref _uri, value);
        }
        #endregion
        #region UDP_MODULE
        private int _udpPort = 9999;
        public int UdpPort
        {
            get => _udpPort;
            set => SetProperty(ref _udpPort, value);
        }
        #endregion
        #region DATA_EXPORT
        private bool _exportEnabled = true;
        private string _exportHost = "127.0.0.1";
        private int _exportPort = 9998;
        public bool ExportEnabled
        {
            get => _exportEnabled;
            set => SetProperty(ref _exportEnabled, value);
        }
        public string ExportHost
        {
            get => _exportHost;
            set => SetProperty(ref _exportHost, value);
        }
        public int ExportPort
        {
            get => _exportPort;
            set => SetProperty(ref _exportPort, value);
        }
        #endregion
    }
}