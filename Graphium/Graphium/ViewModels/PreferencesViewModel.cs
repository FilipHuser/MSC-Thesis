using Graphium.Core;
using Graphium.Interfaces;
using Graphium.Models;
using SharpPcap;

namespace Graphium.ViewModels
{
    internal class PreferencesViewModel : ViewModelBase
    {
        #region SERVICES
        private readonly IAppConfigurationService _appConfigurationService;
        private readonly IViewManager _viewManager;
        #endregion
        #region PROPERTIES
        private AppSettings _settings = new();
        public AppSettings Settings { get => _settings; set => SetProperty(ref _settings, value); }
        public List<string> CaptureDeviceOptions => CaptureDeviceList.Instance.Select(x => x.Description).ToList();
        private string _validationError = string.Empty;
        public string ValidationError { get => _validationError; set => SetProperty(ref _validationError, value); }
        private bool _hasValidationError;
        public bool HasValidationError { get => _hasValidationError; set => SetProperty(ref _hasValidationError, value); }
        #endregion
        #region RELAY_COMMANDS
        public RelayCommand SaveCmd => new RelayCommand(
            execute => Save(),
            canExecute => !HasValidationError);
        #endregion
        #region METHODS
        public PreferencesViewModel(IAppConfigurationService appConfigurationService, IViewManager viewManager)
        {
            _appConfigurationService = appConfigurationService;
            _viewManager = viewManager;
            Init();
        }
        private void Init()
        {
            Settings = _appConfigurationService.GetAppSettings();
            Settings.PropertyChanged += (s, e) => ValidateSettings();
            ValidateSettings();
        }
        private void ValidateSettings()
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(Settings.IPAddr))
                errors.Add("IP Address cannot be empty");
            else if (!IsValidIPv4(Settings.IPAddr))
                errors.Add("Invalid IP Address format (must be xxx.xxx.xxx.xxx)");

            if (string.IsNullOrWhiteSpace(Settings.URI))
                errors.Add("URI cannot be empty");
            else
            {
                string uriToValidate = Settings.URI
                    .Replace("*", "localhost")
                    .Replace("+", "localhost");
                if (!Uri.TryCreate(uriToValidate, UriKind.Absolute, out var uri) ||
                    (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
                    errors.Add("Invalid URI format (must be http:// or https://)");
            }

            if (Settings.PayloadSize < 1 || Settings.PayloadSize > 65535)
                errors.Add("Payload size must be between 1 and 65535");

            if (Settings.CaptureDeviceIndex < 0 || Settings.CaptureDeviceIndex >= CaptureDeviceList.Instance.Count)
                errors.Add("Invalid capture device index");

            if (Settings.UdpPort < 1 || Settings.UdpPort > 65535)
                errors.Add("UDP Port must be between 1 and 65535");

            if (Settings.ExportEnabled)
            {
                if (string.IsNullOrWhiteSpace(Settings.ExportHost) || !IsValidIPv4(Settings.ExportHost))
                    errors.Add("Export Host must be a valid IP address");
                if (Settings.ExportPort < 1 || Settings.ExportPort > 65535)
                    errors.Add("Export Port must be between 1 and 65535");
            }

            if(Settings.ExportEnabled && Settings.ExportPort == Settings.UdpPort)
                errors.Add("Export Port must be different from UDP receive port");

            HasValidationError = errors.Count > 0;
            ValidationError = string.Join(Environment.NewLine, errors);
        }
        private bool IsValidIPv4(string ipAddress)
        {
            var parts = ipAddress.Split('.');
            if (parts.Length != 4) return false;
            foreach (var part in parts)
            {
                if (!byte.TryParse(part, out _)) return false;
                if (part.Length > 1 && part[0] == '0') return false;
            }
            return true;
        }
        private void Save()
        {
            ValidateSettings();
            if (HasValidationError) return;
            try
            {
                _appConfigurationService.SetAppSettings(Settings);
                _viewManager.Close<PreferencesViewModel>();
            }
            catch (Exception ex)
            {
                ValidationError = $"Failed to save settings: {ex.Message}";
                HasValidationError = true;
            }
        }
        #endregion
    }
}