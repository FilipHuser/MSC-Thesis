using System.Collections.ObjectModel;
using Graphium.Enums;
using Graphium.Interfaces;
using Graphium.Models;
using Graphium.Services;
using Graphium.Core;
using System.Collections.Specialized;


namespace Graphium.ViewModels
{
    internal class SignalManagerViewModel : ViewModelBase, IMenuItem
    {
        #region SERVICES
        private readonly IViewManager _viewManager;
        private readonly ISettingsService _settingsService;
        #endregion
        #region PROPERTIES
        public string Header => "Signals";
        public ObservableCollection<SignalBase> Signals { get; set; } = [];
        #endregion
        #region RELAY_COMMANDS
        public RelayCommand RemoveSignalCmd => new RelayCommand(item => RemoveSignal(item));
        public RelayCommand CreateSignalCmd => new RelayCommand(item => SignalCreator());
        #endregion
        #region METHODS
        public SignalManagerViewModel(IViewManager viewManager, ISettingsService settingsService)
        {
            _viewManager = viewManager;
            _settingsService = settingsService;
            Init();
        }
        private void Init()
        {
            var configuredSignals = _settingsService.Load<List<SignalBase>>(SettingsCategory.SIGNALS_CONFIGURATION) ?? new List<SignalBase>();
            Signals = new ObservableCollection<SignalBase>(configuredSignals);
            Signals.CollectionChanged += OnSignalsChanged;
        }
        private void RemoveSignal(object? param)
        {
            if(param is not SignalBase signal) { return; }

            Signals.Remove(signal);
            _settingsService.Save(Signals, SettingsCategory.SIGNALS_CONFIGURATION);
        }
        private void SignalCreator()
        {
            var signal = _viewManager.ShowDialog<DataAcquisitionViewModel,SignalCreatorViewModel, Signal?>(x => x.Signal);
            if(signal == null) { return; }
            Signals.Add(signal);
        }
        private void OnSignalsChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            _settingsService.Save(Signals, SettingsCategory.SIGNALS_CONFIGURATION);
        }
        #endregion
    }
}