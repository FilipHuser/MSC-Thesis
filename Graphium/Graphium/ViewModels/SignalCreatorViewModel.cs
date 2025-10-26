using System.Collections.ObjectModel;
using DataHub.Core;
using Graphium.Core;
using Graphium.Interfaces;
using Graphium.Models;

namespace Graphium.ViewModels
{
    class SignalCreatorViewModel : ViewModelBase
    {
        #region SERVICES
        private readonly IViewManager _viewManager;
        #endregion
        #region PROPERITES
        private bool _isCreating = false;
        private Signal? _signal = new();
        public Signal? Signal { get => _signal; set => SetProperty(ref _signal, value); }
        public ObservableCollection<ModuleType> SourceOptions { get; private set; } = [];
        #endregion
        #region RELAY_COMMANDS
        public RelayCommand CreateCmd => new RelayCommand(execute => Create(), canExecute => !string.IsNullOrWhiteSpace(Signal?.Name));
        public RelayCommand CancelCmd => new RelayCommand(execute => Cancel());
        #endregion
        #region METHODS
        public SignalCreatorViewModel(IViewManager viewManager)
        {
            _viewManager = viewManager;
            Init();
        }
        public void OnSignalClosing()
        {
            if (!_isCreating)
            {
                Signal = null;
            }
        }
        private void Init()
        {
            var sources = Enum.GetValues(typeof(ModuleType)).Cast<ModuleType>().Where(x => x != ModuleType.NONE);
            SourceOptions = new ObservableCollection<ModuleType>(sources);
            Signal!.Source = SourceOptions.First();
        }
        private void Create()
        {
            //TBD => validate if there is no signal with same name
            _isCreating = true;
            _viewManager.Close<SignalCreatorViewModel>();
        }
        private void Cancel()
        {
            Signal = null;
            _viewManager.Close<SignalCreatorViewModel>();
        }
        #endregion
    }
}
