using System.Collections.ObjectModel;
using System.Windows;
using Graphium.Interfaces;
using Graphium.Models;

namespace Graphium.ViewModels
{
    //DATA ACQUISITION CONTROL
    internal class DataAcquisitionWindowVM : ViewModelBase
    {
        #region PROPERTIES
        private IMenuItemViewModel? _selectedItem;
        public IMenuItemViewModel? SelectedItem { get => _selectedItem; set => SetProperty(ref _selectedItem, value); }
        public ObservableCollection<IMenuItemViewModel> MenuItems { get; set; } = [];

        public delegate void SignalConfigCloseEventHandler(List<SignalBase> signals);
        public event SignalConfigCloseEventHandler? SignalConfigCloseRequested;
        #endregion
        public DataAcquisitionWindowVM(Window window , ObservableCollection<SignalBase> signals) : base(window)
        {
            var sccVM = new SignalConfigVM(window , signals);
            sccVM.CloseRequested += signals => { SignalConfigCloseRequested?.Invoke(signals); };

            var scccVM = new SignalCompositeConfigVM(window);

            MenuItems.Add(sccVM);
            MenuItems.Add(scccVM);
            SelectedItem = MenuItems.FirstOrDefault();
        }
        #region METHODS
        #endregion
    }
}
