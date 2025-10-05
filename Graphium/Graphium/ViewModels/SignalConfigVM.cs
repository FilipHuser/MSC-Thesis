  using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using DataHub.Core;
using DataHub.Modules;
using Graphium.Controls;
using Graphium.Core;
using Graphium.Interfaces;
using Graphium.Models;
using Graphium.Views; 
namespace Graphium.ViewModels
{
    //SIGNAL CONFIG
    class SignalConfigVM : ViewModelBase, IMenuItemViewModel
    {
        #region PROPERTIES
        private ModuleType? _selectedSource;
        public string Header => "Signals";
        public UserControl Content { get; }
        public PlotProperties SelectedPlotProperties { get; set; } = new PlotProperties();
        public ModuleType? SelectedSource { get => _selectedSource; set => SetProperty(ref _selectedSource, value); }
        public ObservableCollection<ModuleType> SourceOptions { get; set; } = new ObservableCollection<ModuleType>();
        public ObservableCollection<SignalBase> Signals { get; set; } = new ObservableCollection<SignalBase>();
        public ReadOnlyObservableCollection<Signal> ConcreteSignals { get; }
        public delegate void CloseEventHandler(List<SignalBase> signals);
        public event CloseEventHandler? CloseRequested;
        #endregion
        #region RELAY_COMMANDS
        public RelayCommand CloseCmd => new RelayCommand(execute => Close());
        public RelayCommand CreateSignalCmd => new RelayCommand(execute => CreateSignal()    , canExecute => !string.IsNullOrWhiteSpace(SelectedPlotProperties.Label));
        public RelayCommand RemoveSignalCmd => new RelayCommand(obj =>
        {
            if (obj is Signal signal) { RemoveSignal(signal); }
        });
        #endregion
        public SignalConfigVM(Window window, ObservableCollection<SignalBase> signals) : base(window)  
        {
            Content = new SignalConfigControl(window)
            {
                DataContext = this
            };
            SourceOptions = new ObservableCollection<ModuleType>(Enum.GetValues(typeof(ModuleType)).Cast<ModuleType>());
            SelectedSource = SourceOptions.First();


            var storedSignals = SettingsManager.Load<List<SignalBase>>(SettingsCategory.SIGNALS);

            if (storedSignals != null)
            {
                Signals = new ObservableCollection<SignalBase>(storedSignals);
            }

            var signalCollection = new ObservableCollection<Signal>(Signals.OfType<Signal>());
            ConcreteSignals = new ReadOnlyObservableCollection<Signal>(signalCollection);

            Signals.CollectionChanged += (s, e) =>
            {
                if (e.NewItems != null)
                {
                    foreach (var item in e.NewItems.OfType<Signal>())
                        signalCollection.Add(item);
                }
                if (e.OldItems != null)
                {
                    foreach (var item in e.OldItems.OfType<Signal>())
                        signalCollection.Remove(item);
                }
            };

            Signals.ToList().ForEach(x =>
            {
                var match = signals.FirstOrDefault(y => x.Name == y.Name);
                if (match != null)
                {
                    x.IsAcquired = match.IsAcquired;
                    x.IsPlotted = match.IsPlotted;
                }
            });

        }
        #region METHODS
        private void CreateSignal()
        {
            if (SelectedSource == null) return;
            var properties = (PlotProperties)SelectedPlotProperties.Clone();

            var signal = new Signal(SelectedSource.Value, properties);
            Signals.Add(signal);

            SettingsManager.Save(Signals.ToList(), SettingsCategory.SIGNALS);
            SelectedPlotProperties = new PlotProperties();
        }
        private void RemoveSignal(Signal signal)
        {
            if (signal == null) return;

            Signals.Remove(signal);
            SettingsManager.Save(Signals.ToList(), SettingsCategory.SIGNALS);
        }
        private void Close()
        {
            CloseRequested?.Invoke(Signals.Where(x => x.IsPlotted || x.IsAcquired).ToList());
            Window.Close();
        }
        #endregion
    }
}
