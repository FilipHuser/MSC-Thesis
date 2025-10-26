using System.Runtime.CompilerServices;
using System.Threading.Channels;
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
        private readonly ILoggingService _loggingService;
        #endregion
        #region METHODS
        public ViewManagerService(IViewFactory viewFactory, IViewModelFactory viewModelFactory, ILoggingService loggingService)
        {
            _viewFactory = viewFactory;
            _viewModelFactory = viewModelFactory;
            _loggingService = loggingService;
        }
        public void Show<TOwnerViewModel, TViewModel>() where TViewModel : ViewModelBase where TOwnerViewModel : ViewModelBase
        {
            ShowView<TOwnerViewModel, TViewModel>(false);
        }
        public void ShowDialog<TOwnerViewModel, TViewModel>() where TViewModel : ViewModelBase where TOwnerViewModel : ViewModelBase
        {
            ShowView<TOwnerViewModel, TViewModel>(true);
        }
        public TResult ShowDialog<TOwnerViewModel, TViewModel, TResult>(Func<TViewModel, TResult> resultSelector) 
            where TViewModel : ViewModelBase where TOwnerViewModel : ViewModelBase
        {
            var vm = ShowView<TOwnerViewModel, TViewModel>(modal: true);
            return resultSelector(vm);
        }
        private TViewModel ShowView<TOwnerViewModel, TViewModel>(bool modal) where TViewModel : ViewModelBase where TOwnerViewModel : ViewModelBase
        {
            var vm = _viewModelFactory.Create<TViewModel>();
            var win = _viewFactory.Create<TViewModel>();
           
            var ownerWindow = Application.Current.Windows
                .OfType<Window>()
                .FirstOrDefault(x => x.DataContext is TOwnerViewModel);

            if (ownerWindow != null)
                _loggingService.LogDebug($"Owner window found: {ownerWindow.GetType().Name}");
            else
                _loggingService.LogWarning($"Owner window of type {typeof(TOwnerViewModel).Name} not found");

            win.DataContext = vm;
            win.Owner = ownerWindow;
            win.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            if (modal)
            {
                _loggingService.LogDebug($"Showing {win.GetType().Name} modally");
                win.ShowDialog();
            }
            else
            {
                _loggingService.LogDebug($"Showing {win.GetType().Name} non-modally");
                win.Show();
            }
            return vm;
        }

        public void Close<TViewModel>()
        {
            var window = Application.Current.Windows
                .OfType<Window>()
                .FirstOrDefault(x => x.DataContext is TViewModel);

            if(window == null) { _loggingService.LogWarning($"No window found for {typeof(TViewModel).Name}"); }

            _loggingService.LogDebug($"Closing {window?.GetType().Name} View");
            window?.Close();
        }
        #endregion
    }
}