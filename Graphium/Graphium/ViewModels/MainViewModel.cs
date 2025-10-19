using System.Collections.ObjectModel;
using Graphium.Interfaces;
using Graphium.Core;
using Graphium.Controls;
using Graphium.Services;
using Graphium.Views;


namespace Graphium.ViewModels
{
    internal class MainViewModel : ViewModelBase
    {
        #region SERVICES
        private readonly IViewManager _viewManager;
        private readonly ISignalService _signalService;
        private readonly IViewModelFactory _viewModelFactory;
        #endregion
        #region PROPERTIES
        private MeasurementViewModel? _currentTab;
        public MeasurementViewModel? CurrentTab 
        { 
            get => _currentTab;
            set
            {
                SetProperty(ref _currentTab, value);
                _signalService.SetCurrentSignals(_currentTab!.Signals);
            } 
        }
        public ObservableCollection<MeasurementViewModel> Tabs { get; set; } = [];
        #endregion
        #region RELAY_COMMANDS
        public RelayCommand NewTabCmd => new RelayCommand(execute => NewTab());
        public RelayCommand CloseTabCmd => new RelayCommand(item => CloseTab(item));
        public RelayCommand DataAcquisitionSetupCmd => new RelayCommand(execute => DataAcquisitionSetup() , canExecute => CurrentTab is not null);
        #endregion
        public MainViewModel(IViewManager viewManager, IViewModelFactory viewModelFactory , ISignalService signalServicem)
        {
            _viewManager = viewManager;
            _viewModelFactory = viewModelFactory;
            _signalService = signalServicem;
            NewTab();
        }
        #region METHODS
        private void NewTab()
        {
            int maxId = Tabs.Any() ? Tabs.Max(t => t.TabId) : 0;
            int newId = maxId + 1;
            var tab = _viewModelFactory.Create<MeasurementViewModel>();
            tab.TabId = newId;
            tab.Name = string.Format("Untitled_{0}.gra", tab.TabId);

            Tabs.Add(tab);
            CurrentTab = tab;
        }
        private void CloseTab(object item)
        {
            if(item is not MeasurementViewModel mvm) { return; }
            Tabs.Remove(mvm);
            CurrentTab = !Tabs.Any() ? null : Tabs.Last();
        }
        private void DataAcquisitionSetup()
        {
            _viewManager.Show<DataAcquisitionViewModel>(this,true);
        }
        #endregion
    }
}
