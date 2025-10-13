  using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DataHub.Core;
using DataHub.Modules;
using Graphium.Controls;
using Graphium.Core;
using Graphium.Interfaces;
using Graphium.Models;
using Graphium.Views; 
namespace Graphium.ViewModels
{
    class ChannelsConfigControlVM : ViewModelBase, IMenuItemViewModel
    {
        #region PROPERTIES
        private ModuleType? _selectedSource;
        private int _channelCount;
        public string Header => "Channels";
        public UserControl Content { get; }
        public ObservableCollection<int> Channels { get; set; } = [];
        public ObservableCollection<ChannelSlot> ConfiguredChannels { get; } = [];
        public ObservableCollection<SignalBase> ChannelOptions { get; set; } = [];
        public delegate void CloseEventHandler(List<SignalBase> signals);
        public event CloseEventHandler? CloseRequested;
        #endregion
        #region RELAY_COMMANDS
        public RelayCommand AddChannelCmd => new RelayCommand(execute => Add());
        public RelayCommand RemoveChannelCmd => new RelayCommand(param => Remove(param));
        public RelayCommand SetupCmd => new RelayCommand(execute => Setup(), canExecute => ConfiguredChannels.Any(x => x.Signal != null));
        public RelayCommand SignalChangedCmd => new RelayCommand(param => OnSignalChanged(param));
        #endregion
        public ChannelsConfigControlVM(Window window) : base(window)  
        {
            Content = new ChannelsConfigControl(window)
            {
                DataContext = this
            };

            //TBD => Load available channels from xml
            var ecg = new Signal("ECG", ModuleType.BIOPAC);
            var rsp = new Signal("RESP", ModuleType.BIOPAC);
            var rspr = new SignalComposite("RSP-R", new List<Signal>() { ecg , rsp});
            new List<SignalBase> { ecg, rsp, rspr }.ForEach(s => ChannelOptions.Add(s));
        }
        #region METHODS
        private void Add()
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
        }
        private void Remove(object? param)
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
        }
        private void RefreshAvailableNumbers()
        {
            for (int i = 1; i <= ConfiguredChannels.Count; i++)
            {
                if (!Channels.Contains(i))
                    Channels.Add(i);
            }

            for (int i = Channels.Count; i > ConfiguredChannels.Count; i--)
                Channels.Remove(i);
        }
        private void Setup()
        {
            // Check for duplicate numbers
            var duplicates = ConfiguredChannels
                .GroupBy(c => c.Number)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicates.Any())
            {
                string duplicateList = string.Join(", ", duplicates);
                MessageBox.Show($"Duplicate channel numbers detected: {duplicateList}. Please fix before proceeding.",
                                "Duplicate Channels",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                return;
            }

            CloseRequested?.Invoke(ConfiguredChannels.Where(c => c.Signal != null).Select(c => c.Signal!).ToList());
            Window.Close();
        }
        private void OnSignalChanged(object? param)
        {
            if (param is not ChannelSlot channelSlot || channelSlot.Signal == null) { return; }

            if (channelSlot.Signal is SignalComposite composite)
            {
                //TBD => COMPOSITE SIGNAL CHANNEL SETUP
            }
        }
        #endregion
    }
}
