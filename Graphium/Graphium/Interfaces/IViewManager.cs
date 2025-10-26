using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Graphium.ViewModels;

namespace Graphium.Interfaces
{
    internal interface IViewManager
    {
        #region METHODS
        void Show<TOwnerViewModel, TViewModel>() where TViewModel : ViewModelBase where TOwnerViewModel : ViewModelBase;
        void ShowDialog<TOwnerViewModel, TViewModel>() where TViewModel : ViewModelBase where TOwnerViewModel : ViewModelBase;
        TResult ShowDialog<TOwnerViewModel,TViewModel, TResult>(Func<TViewModel, TResult> resultSelector) where TViewModel : ViewModelBase where TOwnerViewModel : ViewModelBase;
        void Close<TViewModel>();
        #endregion
    }
}
