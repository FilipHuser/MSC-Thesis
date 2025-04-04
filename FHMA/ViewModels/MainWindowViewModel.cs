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
        #region PROPERTIES
        private ObservableCollection<BiometricSignal> _biometricalSignals = new ObservableCollection<BiometricSignal>();
        public ObservableCollection<BiometricSignal> BiometricSignals { get => _biometricalSignals; set => SetProperty(ref _biometricalSignals, value); }
        public int MaxChannels => int.TryParse(ConfigurationManager.AppSettings["MaxChannels"], out int maxChannels) ? maxChannels : 0;
        public string GraphCount => $"{BiometricSignals.Count} / {MaxChannels}";
        #endregion
        #region RELAY_COMMANDS
        public RelayCommand StartCapCommand => new RelayCommand(execute => StartCapturing() , canExecute => BiometricSignals.Count > 0); 
        public RelayCommand AddBSCommand => new RelayCommand(execute => AddBiometricSignal() , canExecute => BiometricSignals.Count < MaxChannels );
        public RelayCommand RemoveBSCommand => new RelayCommand((item) => RemoveBiometricSignal(item));
        public RelayCommand LoadConfCommand => new RelayCommand(execute => LoadConfiguration());
        public RelayCommand SaveConfCommand => new RelayCommand(execute => SaveConfiguration() , canExecute => BiometricSignals.Count > 0);

        #endregion
        public MainWindowViewModel(Window window) : base(window)
        {
            BiometricSignals.CollectionChanged += (object? sender, NotifyCollectionChangedEventArgs e) => { OnPropertyChanged(nameof(GraphCount)); };
        }
        #region METHODS
        private void StartCapturing()
        {
            var mw = new MonitorWindow(BiometricSignals);
            mw.Show();
            Window.Close();
        }
        private void AddBiometricSignal()
        {
            var bscw = new BSConfigWindow();
            bscw.OnGraphAdded += (bs) => {
                BiometricSignals.Add(bs);
            };
            bscw.Owner = Window;
            bscw.ShowDialog();
        }
        private void RemoveBiometricSignal(object item)
        {
            if (item is BiometricSignal bs) { BiometricSignals.Remove(bs); }
        }
        private void LoadConfiguration()
        {
            BiometricSignals = new ObservableCollection<BiometricSignal>(XmlManager.Load<List<BiometricSignal>>("BiometricSignalsConfiguration", "BSConf.xml").First());
            BiometricSignals.SelectMany(x => x.Graphs).ToList().ForEach(x => x.InitPlot());
        }
        private void SaveConfiguration()
        {
            XmlManager.Store("BiometricSignalsConfiguration", "BSConf.xml", BiometricSignals.ToList(), true);
        }
        #endregion
    }
}
