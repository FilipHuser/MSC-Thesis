using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using FHMA.Core;
using FHMA.Models;
using FHMA.Views;

namespace FHMA.ViewModels
{
    internal class MainWindowViewModel : BaseViewModel
    {
        private ObservableCollection<BiometricSignal> _biometricalSignals = new ObservableCollection<BiometricSignal>();
        public ObservableCollection<BiometricSignal> BiometricSignals { get => _biometricalSignals; set => SetProperty(ref _biometricalSignals, value); }
        public int MaxChannels => int.TryParse(ConfigurationManager.AppSettings["MaxChannels"], out int maxChannels) ? maxChannels : 0;
        public string GraphCount => $"{BiometricSignals.Count} / {MaxChannels}";

        #region RELAY_COMMANDS
        public RelayCommand AddBSCommand => new RelayCommand(execute => AddBiometricSignal() , canExecute => BiometricSignals.Count < MaxChannels );
        public RelayCommand RemoveBSCommand => new RelayCommand(execute => RemoveBiometricSignal());
        public RelayCommand LoadConfCommand => new RelayCommand(execute => LoadConfiguration());
        #endregion


        public MainWindowViewModel(Window window) : base(window)
        {
            BiometricSignals.CollectionChanged += (object? sender, NotifyCollectionChangedEventArgs e) => { OnPropertyChanged(nameof(GraphCount)); };

            BiometricSignals.Add(new BiometricSignal() { Name = "ANAL" , Source = FHAPI.Core.FHPacket.PacketSource.BIOPAC});
        }

        #region METHODS
        private void AddBiometricSignal()
        {
            var bscw = new BSConfigWindow();
            bscw.OnGraphAdded += (bs) => {
                BiometricSignals.Add(bs);
            };
            bscw.Owner = Window;
            bscw.ShowDialog();
        }
        private void RemoveBiometricSignal()
        {
        }
        private void LoadConfiguration()
        {
            BiometricSignals = new ObservableCollection<BiometricSignal>(XmlManager.Load<List<BiometricSignal>>("BiometricSignalsConfiguration", "BSConf.xml").First());
        }
        #endregion
    }
}
