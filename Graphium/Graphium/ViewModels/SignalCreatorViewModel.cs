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
        #region PROPERTIES
        private bool _isCreating = false;

        private SignalBase? _signal;
        public SignalBase? Signal
        {
            get => _signal;
            set
            {
                SetProperty(ref _signal, value);
                OnPropertyChanged(nameof(IsNumeric));
            }
        }

        private SignalType _selectedType;
        public SignalType SelectedType
        {
            get => _selectedType;
            set
            {
                SetProperty(ref _selectedType, value);
                CreateSignalForType(value);
            }
        }

        private ModuleType _selectedSource;
        public ModuleType SelectedSource
        {
            get => _selectedSource;
            set
            {
                SetProperty(ref _selectedSource, value);
                if (Signal != null) Signal.Source = value;
            }
        }

        public bool IsNumeric => Signal is NumericSignal;

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
                Signal = null;
        }
        private void Init()
        {
            var types = Enum.GetValues(typeof(SignalType)).Cast<SignalType>().Where(x => x != SignalType.NaN);
            var sources = Enum.GetValues(typeof(ModuleType)).Cast<ModuleType>().Where(x => x != ModuleType.NaN);
            TypeOptions = new ObservableCollection<SignalType>(types);
            SourceOptions = new ObservableCollection<ModuleType>(sources);
            SelectedSource = SourceOptions.First();
            SelectedType = TypeOptions.First();
        }
        private void CreateSignalForType(SignalType type)
        {
            var previousName = Signal?.Name;

            Signal = type switch
            {
                SignalType.Numeric => new NumericSignal(),
                SignalType.Text => new TextSignal(),
                _ => null
            };

            if (Signal != null)
            {
                Signal.Source = SelectedSource;
                if (!string.IsNullOrEmpty(previousName))
                    Signal.Name = previousName;
            }
        }
        private void Create()
        {
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