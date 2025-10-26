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
        void Show<TViewModel>(ViewModelBase owner) where TViewModel : ViewModelBase;
        void ShowDialog<TViewModel>(ViewModelBase owner) where TViewModel: ViewModelBase;
        TResult ShowDialog<TViewModel, TResult>(ViewModelBase owner, Func<TViewModel, TResult> resultSelector) where TViewModel : ViewModelBase;
        #endregion
    }
}
