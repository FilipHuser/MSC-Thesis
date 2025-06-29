using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using Graphium.Core;
using Graphium.Models;
using Graphium.Views;

namespace Graphium.ViewModels
{
    internal class MainWindowViewModel : BaseViewModel
    {
        #region PROPERTIES
        private ObservableCollection<BiologicalSignal> _BiologicalalSignals = new ObservableCollection<BiologicalSignal>();
        public ObservableCollection<BiologicalSignal> BiologicalSignals { get => _BiologicalalSignals; set => SetProperty(ref _BiologicalalSignals, value); }
        public string GraphCount => $"{BiologicalSignals.Count}";
        #endregion
        #region RELAY_COMMANDS
        public RelayCommand StartCapCommand => new RelayCommand(execute => StartCapturing() , canExecute => BiologicalSignals.Count > 0); 
        public RelayCommand AddBSCommand => new RelayCommand(execute => AddBiologicalSignal());
        public RelayCommand RemoveBSCommand => new RelayCommand((item) => RemoveBiologicalSignal(item));
        public RelayCommand LoadConfCommand => new RelayCommand(execute => LoadConfiguration());
        public RelayCommand SaveConfCommand => new RelayCommand(execute => SaveConfiguration() , canExecute => BiologicalSignals.Count > 0);

        #endregion
        public MainWindowViewModel(Window window) : base(window)
        {
            BiologicalSignals.CollectionChanged += (object? sender, NotifyCollectionChangedEventArgs e) => { OnPropertyChanged(nameof(GraphCount)); };
        }
        #region METHODS
        private void StartCapturing()
        {
            var mw = new MonitorWindow(BiologicalSignals);
            mw.Show();
            Window.Close();
        }
        private void AddBiologicalSignal()
        {
            var bscw = new BSConfigWindow();
            bscw.OnGraphAdded += (bs) => {
                BiologicalSignals.Add(bs);
            };
            bscw.Owner = Window;
            bscw.ShowDialog();
        }
        private void RemoveBiologicalSignal(object item)
        {
            if (item is BiologicalSignal bs) { BiologicalSignals.Remove(bs); }
        }
        private void LoadConfiguration()
        {
            BiologicalSignals = new ObservableCollection<BiologicalSignal>(XmlManager.Load<List<BiologicalSignal>>("BiologicalSignalsConfiguration", "BSConf.xml").First());
        }
        private void SaveConfiguration()
        {
            XmlManager.Store("BiologicalSignalsConfiguration", "BSConf.xml", BiologicalSignals.ToList(), true);
        }
        #endregion
    }
}
