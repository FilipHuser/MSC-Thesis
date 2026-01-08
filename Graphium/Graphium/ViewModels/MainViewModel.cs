using System.Collections.ObjectModel;
using Graphium.Interfaces;
using Graphium.Core;
using Graphium.Controls;
using Graphium.Services;
using Graphium.Views;
using HarfBuzzSharp;
using System.Runtime.CompilerServices;


namespace Graphium.ViewModels
{
    internal class MainViewModel : ViewModelBase
    {
        #region SERVICES
        private readonly IViewManager _viewManager;
        private readonly ISignalService _signalService;
        private readonly IViewModelFactory _viewModelFactory;
        private readonly ILoggingService _loggingService;
        #endregion
        #region PROPERTIES
        private MeasurementViewModel? _currentTab;
        public MeasurementViewModel? CurrentTab
        {
            get => _currentTab;
            set
            {
                if (_currentTab == value) return;

                _signalService.SignalsChanged -= OnSignalsChanged;

                SetProperty(ref _currentTab, value);

                if (_currentTab != null)
                {
                    _signalService.SetCurrentSignals(_currentTab.Signals);
                    _signalService.SignalsChanged += OnSignalsChanged;
                }
            }
        }
        public ObservableCollection<MeasurementViewModel> Tabs { get; set; } = [];
        public event EventHandler? CurrentTabChanged; 
        #endregion
        #region RELAY_COMMANDS
        public RelayCommand NewTabCmd => new RelayCommand(execute => NewTab());
        public RelayCommand CloseTabCmd => new RelayCommand(item => CloseTab(item));
        public RelayCommand NextTabCmd => new RelayCommand(execute => NextTab(), canExecute => CurrentTab != null && Tabs.Count > 1);
        public RelayCommand PreviousTabCmd => new RelayCommand(execute => PreviousTab(), canExecute => CurrentTab != null && Tabs.Count > 1);
        public RelayCommand DataAcquisitionSetupCmd => new RelayCommand(execute => DataAcquisitionSetup(), canExecute => CurrentTab is not null);
        public RelayCommand PreferencesCmd => new RelayCommand(execute => Preferences());
        #endregion
        public MainViewModel(IViewManager viewManager, IViewModelFactory viewModelFactory , ISignalService signalService, ILoggingService loggingService)
        {
            _viewManager = viewManager;
            _viewModelFactory = viewModelFactory;
            _signalService = signalService;
            _loggingService = loggingService;
            _loggingService.LogDebug("Application started");
            NewTab();
        }
        #region METHODS
        private void NewTab()
        {
            int maxId = Tabs.Any() ? Tabs.Max(t => t.TabId) : 0;
            int newId = maxId + 1;
            var tab = _viewModelFactory.Create<MeasurementViewModel>();
            tab.TabId = newId;
            tab.Name = string.Format("Untitled{0}", tab.TabId);
            Tabs.Add(tab);
            CurrentTab = tab;
            _loggingService.LogDebug($"Measurement Tab added: '{tab.Name}'");
        }
        private void CloseTab(object item)
        {
            if(item is not MeasurementViewModel mvm) { return; }
            Tabs.Remove(mvm);
            CurrentTab = !Tabs.Any() ? null : Tabs.Last();
            _loggingService.LogDebug($"Measurement Tab closed: '{mvm.Name}'");
        }
        private void NextTab()
        {
            int currentIndex = Tabs.IndexOf(CurrentTab!);
            int nextIndex = (currentIndex + 1) % Tabs.Count;
            CurrentTab = Tabs[nextIndex];
            _loggingService.LogDebug($"Switched to next Measurement tab, current: '{CurrentTab?.Name}'");
        }
        private void PreviousTab()
        {
            int currentIndex = Tabs.IndexOf(CurrentTab!);
            int prevIndex = (currentIndex - 1 + Tabs.Count) % Tabs.Count;
            CurrentTab = Tabs[prevIndex];
            _loggingService.LogDebug($"Switched to previous Measurement tab, current: '{CurrentTab?.Name}'");
        }
        private void DataAcquisitionSetup() => _viewManager.ShowDialog<MainViewModel, DataAcquisitionViewModel>();
        private void Preferences() => _viewManager.ShowDialog<MainViewModel, PreferencesViewModel>();
        private void OnSignalsChanged(object? sender, EventArgs e)  
        {
            _currentTab?.DataPlotter.OnSignalsChanged();
        }
        #endregion
    }
}
