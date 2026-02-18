using System.Collections.ObjectModel;
using DataHub.Core;
using Graphium.Core;
using Graphium.Enums;
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
        private SignalBase? _signal;
        public SignalBase? Signal { get => _signal; set => SetProperty(ref _signal, value); }
        public ObservableCollection<SignalType> TypeOptions { get; private set; } = [];
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
            var types = Enum.GetValues(typeof(SignalType)).Cast<SignalType>().Where(x => x != SignalType.NaN);
            var sources = Enum.GetValues(typeof(ModuleType)).Cast<ModuleType>().Where(x => x != ModuleType.NaN);
            TypeOptions = new ObservableCollection<SignalType>(types);
            SourceOptions = new ObservableCollection<ModuleType>(sources);

            Signal!.Type = TypeOptions.First();
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
