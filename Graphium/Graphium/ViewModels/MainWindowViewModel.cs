using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using Graphium.Core;
using Graphium.Models;
using Graphium.Views;

namespace Graphium.ViewModels
{
    internal class MainWindowViewModel : BaseViewModel
    {
        #region PROPERTIES
        private ObservableCollection<SignalBase> _Signals = new ObservableCollection<SignalBase>();
        public ObservableCollection<SignalBase> Signals { get => _Signals; set => SetProperty(ref _Signals, value); }
        public string SignalCount => $"{Signals.Count}";
        #endregion
        #region RELAY_COMMANDS
        public RelayCommand StartCapCommand => new RelayCommand(execute => StartCapturing() , canExecute => Signals.Count > 0); 
        public RelayCommand AddSignalCommand => new RelayCommand(execute => AddSignal());
        public RelayCommand RemoveSignalCommand => new RelayCommand((item) => RemoveSignal(item));
        public RelayCommand LoadConfCommand => new RelayCommand(execute => LoadConfiguration() , canExecute => false);
        public RelayCommand SaveConfCommand => new RelayCommand(execute => SaveConfiguration() , canExecute => false/*Signals.Count > 0*/);

        #endregion
        public MainWindowViewModel(Window window) : base(window)
        {
            Signals.CollectionChanged += (object? sender, NotifyCollectionChangedEventArgs e) => { OnPropertyChanged(nameof(SignalCount)); };
        }
        #region METHODS
        private void StartCapturing()
        {
            var mw = new MonitorWindow(Signals);
            mw.Show();
            Window.Close();
        }
        private void AddSignal()
        {
            var scw = new SignalConfigWindow();
            scw.OnSignalAdded += (signal) => { Signals.Add(signal); };
            scw.Owner = Window;
            scw.ShowDialog();
        }
        private void RemoveSignal(object item)
        {
            if (item is SignalBase signal) { Signals.Remove(signal); }
        }
        private void LoadConfiguration()
        {
            //BiologicalSignals = new ObservableCollection<BiologicalSignal>(XmlManager.Load<List<BiologicalSignal>>("BiologicalSignalsConfiguration", "BSConf.xml").First());
        }
        private void SaveConfiguration()
        {
            //XmlManager.Store("BiologicalSignalsConfiguration", "BSConf.xml", BiologicalSignals.ToList(), true);
        }
        #endregion
    }
}
