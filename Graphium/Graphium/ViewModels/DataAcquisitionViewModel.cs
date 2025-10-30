using System.Collections.ObjectModel;
using Graphium.Interfaces;

namespace Graphium.ViewModels
{
    internal class DataAcquisitionViewModel : ViewModelBase
    {
        #region SERVICES
        private readonly ISignalService _signalService;
        private readonly IViewModelFactory _viewModelFactory;
        #endregion
        #region PROPERTIES
        private ViewModelBase? _currentMenuItem;
        public ViewModelBase? CurrentMenuItem { get => _currentMenuItem; set => SetProperty(ref _currentMenuItem, value); }
        public ObservableCollection<ViewModelBase> MenuItems { get; set; }
        #endregion
        #region METHODS
        public DataAcquisitionViewModel(ISignalService signalService , IViewModelFactory viewModelFactory)
        {
            _signalService = signalService;
            _viewModelFactory = viewModelFactory;

            var ccvm = _viewModelFactory.Create<ChannelsConfigViewModel>();
            var smvm = _viewModelFactory.Create<SignalManagerViewModel>();

            MenuItems = new ObservableCollection<ViewModelBase>() { ccvm, smvm };
            _currentMenuItem = MenuItems.First();
        }
        #endregion
    }
}
