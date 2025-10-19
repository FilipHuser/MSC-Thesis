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
        void Show<TViewModel>(ViewModelBase owner, bool modal = false) where TViewModel : ViewModelBase;
        #endregion
    }
}
