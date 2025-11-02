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
        #endregion
        #region RELAY_COMMANDS
        public RelayCommand SaveCmd => new RelayCommand(execute => Save());
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
        }
        private void Save()
        {
            _appConfigurationService.SetAppSettings(Settings);
            _viewManager.Close<PreferencesViewModel>();
        }
        #endregion
    }
}
