  using System;
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
        public string Header => "Channels";
        public UserControl Content { get; }
        public ObservableCollection<int> AvailableChannelNumbers { get; } = new ObservableCollection<int>();

        public ObservableCollection<ChannelSlot> ConfiguredChannels { get; set; } = new ObservableCollection<ChannelSlot>();
        public ObservableCollection<SignalBase> ChannelOptions { get; set; } = new ObservableCollection<SignalBase>();
        public delegate void CloseEventHandler(List<SignalBase> signals);
        public event CloseEventHandler? CloseRequested;
        #endregion
        #region RELAY_COMMANDS
        public RelayCommand RemoveChannelCmd => new RelayCommand(execute =>
        {
            if (execute is ChannelSlot slot)
                RemoveChannel(slot);
        },
        canExecute =>
        {
            if (canExecute is not ChannelSlot slot)
                return false;

            if (ConfiguredChannels.Count <= 1)
                return false;

            if (slot == ConfiguredChannels.Last() && slot.Signal == null)
                return false;

            return true;
        });

        #endregion
        public ChannelsConfigControlVM(Window window) : base(window)  
        {
            Content = new ChannelsConfigControl(window)
            {
                DataContext = this
            };

            AddChannel();

            //TBD => Load available channels from xml
            var ecg = new Signal("ECG", ModuleType.BIOPAC);
            var rsp = new Signal("RESP", ModuleType.BIOPAC);
            var rspr = new SignalComposite("RSP-R", new List<Signal>() { ecg , rsp});
            new List<SignalBase> { ecg, rsp, rspr }.ForEach(s => ChannelOptions.Add(s));
        }
        #region METHODS
        private void AddChannel()
        {
            var slot = new ChannelSlot() { Number = ConfiguredChannels.Count + 1, Signal = null };

            slot.PropertyChanged += (s, e) =>
            {
                switch(e.PropertyName)
                {
                    case nameof(ChannelSlot.Number):
                        var duplicate = ConfiguredChannels.Any(c => c != slot && c.Number == slot.Number);
                        if (duplicate)
                        {
                            MessageBox.Show("This channel number is already used. Please select another one.", "Duplicate Channel");
                            slot.Number = ConfiguredChannels.IndexOf(slot) + 1;
                        }
                        break;
                        
                    case nameof(ChannelSlot.Signal):
                        if (slot.Signal is SignalComposite sc)
                        {
                            //TBD => DIALOG HERE TO SET THE CHANNELS OF THE COMPOSITE
                        }
                        if (slot == ConfiguredChannels.Last() && slot.Signal != null)
                        {
                            AddChannel();
                        }
                        break;
                }
            };

            ConfiguredChannels.Add(slot);
        }
        private void RemoveChannel(ChannelSlot slot)
        {
            ConfiguredChannels.Remove(slot);
            for (int i = 0; i < ConfiguredChannels.Count; i++)
            {
                ConfiguredChannels[i].Number = i + 1;
            }
            CommandManager.InvalidateRequerySuggested();
        }
        private void Close()
        {
            //CloseRequested?.Invoke(ConfiguredChannels.Where(x => x.IsPlotted || x.IsAcquired).ToList());
            Window.Close();
        }
        #endregion
    }
}
