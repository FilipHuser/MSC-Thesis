using System.Collections.ObjectModel;
using Graphium.Enums;
using Graphium.Interfaces;
using Graphium.Models;
using Graphium.Core;
using System.Collections.Specialized;


namespace Graphium.ViewModels
{
    internal class SignalManagerViewModel : ViewModelBase, IMenuItem
    {
        #region SERVICES
        private readonly IViewManager _viewManager;
        private readonly IConfigurationService _ConfigurationService;
        #endregion
        #region PROPERTIES
        public string Header => "Signals";
        public ObservableCollection<Signal> Signals { get; set; } = [];
        #endregion
        #region RELAY_COMMANDS
        public RelayCommand RemoveSignalCmd => new RelayCommand(item => RemoveSignal(item));
        public RelayCommand CreateSignalCmd => new RelayCommand(item => SignalCreator());
        #endregion
        #region METHODS
        public SignalManagerViewModel(IViewManager viewManager, IConfigurationService ConfigurationService)
        {
            _viewManager = viewManager;
            _ConfigurationService = ConfigurationService;
            Init();
        }
        private void Init()
        {
            var configuredSignals = _ConfigurationService.Load<List<Signal>>(SettingsCategory.SIGNALS_CONFIGURATION) ?? new List<Signal>();
            Signals = new ObservableCollection<Signal>(configuredSignals);
            Signals.CollectionChanged += OnSignalsChanged;
        }
        private void RemoveSignal(object? param)
        {
            if(param is not Signal signal) { return; }

            Signals.Remove(signal);
            _ConfigurationService.Save(Signals, SettingsCategory.SIGNALS_CONFIGURATION);
        }
        private void SignalCreator()
        {
            var signal = _viewManager.ShowDialog<DataAcquisitionViewModel,SignalCreatorViewModel, Signal?>(x => x.Signal);
            if(signal == null) { return; }
            Signals.Add(signal);
        }
        private void OnSignalsChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            _ConfigurationService.Save(Signals, SettingsCategory.SIGNALS_CONFIGURATION);
        }
        #endregion
    }
}