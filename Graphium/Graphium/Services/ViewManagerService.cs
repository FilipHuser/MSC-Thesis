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
        public ViewManagerService(IViewFactory viewFactory, IViewModelFactory viewModelFactory)
        {
            _viewFactory = viewFactory;
            _viewModelFactory = viewModelFactory;
        }

        public void Show<TViewModel>(ViewModelBase owner) where TViewModel : ViewModelBase
        {
            ShowView<TViewModel>(owner, modal: false);
        }

        public void ShowDialog<TViewModel>(ViewModelBase owner) where TViewModel : ViewModelBase
        {
            ShowView<TViewModel>(owner, modal: true);
        }

        public TResult ShowDialog<TViewModel, TResult>(ViewModelBase owner, Func<TViewModel, TResult> resultSelector)
            where TViewModel : ViewModelBase
        {
            var vm = ShowView<TViewModel>(owner, modal: true);
            return resultSelector(vm);
        }

        private TViewModel ShowView<TViewModel>(ViewModelBase owner, bool modal) where TViewModel : ViewModelBase
        {
            var vm = _viewModelFactory.Create<TViewModel>();
            var win = _viewFactory.Create<TViewModel>();
            var ownerWindow = Application.Current.Windows
                .OfType<Window>()
                .FirstOrDefault(x => ReferenceEquals(x.DataContext, owner));

            win.DataContext = vm;
            win.Owner = ownerWindow;
            win.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            if (modal)
            {
                win.ShowDialog();
            } else {
                win.Show();
            }
            return vm;
        }
        #endregion
    }
}