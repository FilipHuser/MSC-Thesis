using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Windows;
using DataHub.Modules;
using SharpPcap;

namespace Graphium.ViewModels
{
    public class CaptureSettings
    {
        public int CaptureDeviceIndex { get; set; }
        public int PayloadSize { get; set; }
        public int ReadTimeout { get; set; }
        public string? BiopacIPAddress { get; set; }
        public string? HttpURI { get; set; }
    }
    internal class PreferencesWindowVM : ViewModelBase
    {
        #region PROPERTIES

        private int _selectedCaptureDevice;
        public int SelectedCaptureDevice
        {
            get => _selectedCaptureDevice;
            set => SetProperty(ref _selectedCaptureDevice, value);
        }

        public List<string> CaptureDeviceOptions =>
            CaptureDeviceList.Instance.Select(x => x.Description).ToList();

        private int _payloadSize;
        public int PayloadSize
        {
            get => _payloadSize;
            set => SetProperty(ref _payloadSize, value);
        }

        private int _readTimeout;
        public int ReadTimeout
        {
            get => _readTimeout;
            set => SetProperty(ref _readTimeout, value);
        }

        private string? _biopacIPAddress;
        public string? BiopacIPAddress
        {
            get => _biopacIPAddress;
            set => SetProperty(ref _biopacIPAddress, value);
        }

        private string? _httpURI;
        public string? HttpURI
        {
            get => _httpURI;
            set => SetProperty(ref _httpURI, value);
        }

        #endregion
        public PreferencesWindowVM(Window window) : base(window)
        {
            var getAppSetting = (string key) => ConfigurationManager.AppSettings[key];

            int.TryParse(getAppSetting("CaptureDeviceIndex"), out int captureDeviceIndex);
            int.TryParse(getAppSetting("PayloadSize"), out int payloadSize);
            int.TryParse(getAppSetting("ReadTimeout"), out int readTimeout);
            var ipAddr = getAppSetting("IPAddr");
            var uri = getAppSetting("URI");

            SelectedCaptureDevice = captureDeviceIndex >= 0 && captureDeviceIndex < CaptureDeviceOptions.Count
                                    ? captureDeviceIndex
                                    : 0;

            PayloadSize = payloadSize;
            ReadTimeout = readTimeout;
            BiopacIPAddress = ipAddr;
            HttpURI = uri;
        }
        #region METHODS
        public CaptureSettings GetCurrentSettings()
        {
            return new CaptureSettings
            {
                CaptureDeviceIndex = SelectedCaptureDevice,
                PayloadSize = PayloadSize,
                ReadTimeout = ReadTimeout,
                BiopacIPAddress = BiopacIPAddress,
                HttpURI = HttpURI
            };
        }
        #endregion
    }
}
