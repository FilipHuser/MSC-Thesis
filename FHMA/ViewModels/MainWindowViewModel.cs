using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.Runtime.CompilerServices;
using FHMA.Core;
using FHMA.Models;

namespace FHMA.ViewModels
{
    internal class MainWindowViewModel : BaseViewModel
    {
        private ObservableCollection<BiometricSignal> _biometricalSignals = new ObservableCollection<BiometricSignal>();
        public ObservableCollection<BiometricSignal> BiometricSignals { get => _biometricalSignals; set => SetProperty(ref _biometricalSignals, value); }
        public int MaxChannels
        {
            get
            {
                int.TryParse(ConfigurationManager.AppSettings["MaxChannels"], out int maxChannels);
                return maxChannels;
            }
        }
        public string GraphCount => $"{BiometricSignals.Count} / {MaxChannels}";
        public MainWindowViewModel()
        {
            BiometricSignals.CollectionChanged += (object? sender, NotifyCollectionChangedEventArgs e) => { OnPropertyChanged(nameof(GraphCount)); };
        }

        public void Refresh()
        {
            BiometricSignals = new ObservableCollection<BiometricSignal>(XmlSettingsManager.LoadSettings<List<BiometricSignal>>("BiometricSignalsConfiguration" , "BSConf.xml").First());
        }
    }
}
