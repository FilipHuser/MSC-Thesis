using System.Collections.ObjectModel;
using System.Windows;
using Graphium.Interfaces;
using Graphium.Models;

namespace Graphium.ViewModels
{
    internal class DataAcquisitionConfigWindowVM : ViewModelBase
    {
        #region PROPERTIES
        private IMenuItemViewModel? _selectedItem;
        public IMenuItemViewModel? SelectedItem { get => _selectedItem; set => SetProperty(ref _selectedItem, value); }
        public ObservableCollection<IMenuItemViewModel> MenuItems { get; set; } = [];

        public delegate void SignalConfigCloseEventHandler(List<SignalBase> signals);
        public event SignalConfigCloseEventHandler? SignalConfigCloseRequested;
        #endregion
        public DataAcquisitionConfigWindowVM(Window window , ObservableCollection<SignalBase> signals) : base(window)
        {
            var sccVM = new SignalConfigControlVM(window , signals);
            sccVM.CloseRequested += signals => { SignalConfigCloseRequested?.Invoke(signals); };

            MenuItems.Add(sccVM);
            SelectedItem = MenuItems.FirstOrDefault();
        }
        #region METHODS
        #endregion
    }
}
