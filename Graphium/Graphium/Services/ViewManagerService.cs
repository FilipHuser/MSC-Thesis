using System.Windows;
using Graphium.Controls;
using Graphium.Interfaces;
using Graphium.ViewModels;
using Graphium.Views;

namespace Graphium.Services
{
    internal class ViewManagerService : IViewManager
    {
        #region PROPERTIES
        private readonly IViewFactory _viewFactory;
        private readonly IViewModelFactory _viewModelFactory;
        #endregion
        #region METHODS
        public ViewManagerService(IViewFactory viewFactory , IViewModelFactory viewModelFactory)
        {
            _viewFactory = viewFactory;
            _viewModelFactory = viewModelFactory;
        }
        public void Show<TViewModel>(ViewModelBase owner, bool modal = false) where TViewModel : ViewModelBase
        {
            var vm = _viewModelFactory.Create<TViewModel>();
            var win = _viewFactory.Create<TViewModel>();

            var ownerWindow = Application.Current.Windows
                .OfType<Window>()
                .FirstOrDefault(x => ReferenceEquals(x.DataContext, owner));

            win.DataContext = vm;
            win.Owner = ownerWindow;
            win.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            Action show = modal ? new Action(() => win.ShowDialog()) : new Action(win.Show);
            show();
        }
        #endregion
    }
}
