using System.Collections.ObjectModel;
using System.Threading.Channels;
using DataHub.Core;
using Graphium.Core;
using Graphium.Enums;
using Graphium.Interfaces;
using Graphium.Models;
using Graphium.Services;

namespace Graphium.ViewModels
{
    internal class ChannelsConfigViewModel : ViewModelBase, IMenuItem
    {
        #region SERVICES
        private readonly ISignalService _signalService;
        private readonly ILoggingService _loggingService;
        private readonly ISettingsService _settingsService;
        #endregion
        #region PROPERTIES
        public string Header => "Channels";
        public ObservableCollection<int> Channels { get; set; } = [];
        public ObservableCollection<SignalBase> ChannelOptions { get; set; } = [];
        public ObservableCollection<ChannelSlot> ConfiguredChannels { get; set; } = [];
        #endregion
        #region RELAY_COMMANDS
        public RelayCommand AddChannelSlotCmd => new RelayCommand(execute => AddChannelSlot());
        public RelayCommand RemoveChannelSlotCmd => new RelayCommand(item => RemoveChannelSlot(item));
        public RelayCommand SetupCmd => new RelayCommand(execute => Setup());
        #endregion
        #region METHODS
        public ChannelsConfigViewModel(ISignalService signalService, ILoggingService loggingService, ISettingsService settingsService)
        {
            _signalService = signalService;
            _loggingService = loggingService;
            _settingsService = settingsService;
            Init();
        }
        private void Init() 
        {
            var availableSignals = _settingsService.Load<List<SignalBase>>(SettingsCategory.SIGNALS_CONFIGURATION) ?? new List<SignalBase>();
            ChannelOptions = new ObservableCollection<SignalBase>(availableSignals);

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
        }
        #endregion
    }
}
