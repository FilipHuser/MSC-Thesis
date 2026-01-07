using System.Collections.ObjectModel;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Channels;
using DataHub.Core;
using Graphium.Core;
using Graphium.Enums;
using Graphium.Interfaces;
using Graphium.Models;

namespace Graphium.ViewModels
{
    internal class ChannelsConfigViewModel : ViewModelBase, IMenuItem
    {
        #region SERVICES
        private readonly ISignalService _signalService;
        private readonly ILoggingService _loggingService;
        private readonly IConfigurationService _ConfigurationService;
        private readonly IViewManager _viewManager;
        #endregion
        #region PROPERTIES
        private ObservableCollection<ChannelSlot> _configuredChannels = [];
        public string Header => "Channels";
        public ObservableCollection<int> Channels { get; set; } = [];
        public ObservableCollection<Signal> ChannelOptions { get; set; } = [];
        public ObservableCollection<ChannelSlot> ConfiguredChannels { get => _configuredChannels; set => SetProperty(ref _configuredChannels, value); }
        #endregion
        #region RELAY_COMMANDS
        public RelayCommand AddChannelSlotCmd => new RelayCommand(execute => AddChannelSlot());
        public RelayCommand RemoveChannelSlotCmd => new RelayCommand(item => RemoveChannelSlot(item));
        public RelayCommand SetupCmd => new RelayCommand(execute => Setup());

        public RelayCommand SaveConfigurationCmd => new RelayCommand(execute => SaveConfiguration(), canExecute => ConfiguredChannels.Any(x => x.Signal != null));
        public RelayCommand LoadConfigurationCmd => new RelayCommand(execute => LoadConfiguration());

        #endregion
        #region METHODS
        public ChannelsConfigViewModel(ISignalService signalService, ILoggingService loggingService, IConfigurationService ConfigurationService, IViewManager viewManager)
        {
            _signalService = signalService;
            _loggingService = loggingService;
            _ConfigurationService = ConfigurationService;
            _viewManager = viewManager;
            Init();
        }
        public void LoadSignals()
        {
            var availableSignals = _ConfigurationService.Load<List<Signal>>(SettingsCategory.SIGNALS_CONFIGURATION) ?? new List<Signal>();
            var currentSelections = ConfiguredChannels
                .Where(slot => slot.Signal != null)
                .ToDictionary(slot => slot.Number, slot => slot.Signal!.Name);

            ChannelOptions.Clear();
            foreach (var signal in availableSignals)
            {
                ChannelOptions.Add(signal);
            }

            foreach (var slot in ConfiguredChannels)
            {
                if (currentSelections.TryGetValue(slot.Number, out var signalName))
                {
                    var matchingSignal = ChannelOptions.FirstOrDefault(s => s.Name == signalName);
                    slot.Signal = matchingSignal;
                }
            }
        }
        private void Init() 
        {
            LoadSignals();
            var currentConfiguration = _signalService.Signals;
            if(currentConfiguration == null) { return; }

            ConfiguredChannels = new ObservableCollection<ChannelSlot>(
                currentConfiguration.Select((signal, index) => new ChannelSlot
                {
                    Signal = ChannelOptions.FirstOrDefault(s => s.Name == signal.Name),
                    Number = ++index
                })
            );
        }
        private void AddChannelSlot()
        {
            var usedNumbers = ConfiguredChannels
                .Select(c => c.Number)
                .ToHashSet();

            int nextNumber = 1;
            while (usedNumbers.Contains(nextNumber))
                nextNumber++;

            var slot = new ChannelSlot
            {
                Number = nextNumber
            };

            ConfiguredChannels.Add(slot);

            RefreshAvailableNumbers();
            _loggingService.LogDebug($"Channel slot added");
        }
        private void RemoveChannelSlot(object? param)
        {
            if (param is not ChannelSlot cs) return;

            int? removedNumber = cs.Number;

            ConfiguredChannels.Remove(cs);

            if (removedNumber.HasValue)
            {
                foreach (var channel in ConfiguredChannels)
                {
                    if (channel.Number > removedNumber.Value)
                        channel.Number--;
                }
            }

            RefreshAvailableNumbers();
            _loggingService.LogDebug($"Channel slot removed");
        }
        private void RefreshAvailableNumbers()
        {
            for (int i = 1; i <= ConfiguredChannels.Count; i++)
            {
                if (!Channels.Contains(i))
                    Channels.Add(i);
            }

            for (int i = Channels.Count; i > ConfiguredChannels.Count; i--) { Channels.Remove(i); }
        }
        private void Setup()
        {
            if (_signalService.Signals == null) return;
            _signalService.Clear();
            foreach (var slot in ConfiguredChannels)
            {
                if (slot.Signal != null)
                    _signalService.AddSignal(slot.Signal);
            }
            _viewManager.Close<DataAcquisitionViewModel>();
        }

        private void SaveConfiguration()
        {
            _ConfigurationService.Save(ConfiguredChannels.ToList(), SettingsCategory.CHANNELS_CONFIGURATION);
        }
        private void LoadConfiguration()
        {
            var channelSlots = _ConfigurationService.Load<List<ChannelSlot>>(SettingsCategory.CHANNELS_CONFIGURATION);

            if(channelSlots == null || channelSlots.Count == 0) { return; }

            ConfiguredChannels = new ObservableCollection<ChannelSlot>(channelSlots);
        }
        #endregion
    }
}
