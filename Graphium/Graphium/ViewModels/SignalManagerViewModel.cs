using System.Collections.ObjectModel;
using DataHub.Core;
using System.Threading.Channels;
using Graphium.Enums;
using Graphium.Interfaces;
using Graphium.Models;
using Graphium.Services;
using Graphium.Core;


namespace Graphium.ViewModels
{
    internal class SignalManagerViewModel : ViewModelBase, IMenuItem
    {
        #region SERVICES
        private readonly ISettingsService _settingsService;
        #endregion
        #region PROPERTIES
        public string Header => "Signals";
        public ObservableCollection<SignalBase> Signals { get; set; } = [];
        #endregion
        #region RELAY_COMMANDS
        public RelayCommand RemoveSignalCmd => new RelayCommand(item => RemoveSignal(item));
        #endregion
        #region METHODS
        public SignalManagerViewModel(ISettingsService settingsService)
        {
            _settingsService = settingsService;
            Init();
        }
        private void Init()
        {
            var configuredSignals = _settingsService.Load<List<SignalBase>>(SettingsCategory.SIGNALS_CONFIGURATION) ?? new List<SignalBase>();
            Signals = new ObservableCollection<SignalBase>(configuredSignals);
        }
        private void RemoveSignal(object? param)
        {
            if(param is not SignalBase signal) { return; }

            Signals.Remove(signal);
            _settingsService.Save(Signals, SettingsCategory.SIGNALS_CONFIGURATION);
        }
        #endregion
    }
}