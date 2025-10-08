using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Graphium.Interfaces;

namespace Graphium.ViewModels
{
    internal class SignalCompositeConfigControlVM : ViewModelBase, IMenuItemViewModel
    {
        #region PROPERTIES
        public string Header => "Composite Signals";
        public UserControl Content { get; }
        #endregion
        public SignalCompositeConfigControlVM(Window parent) : base(parent)
        {
            Content = new SignalCompositeConfigControl(parent)
            {
                DataContext = this
            };
        }
        #region METHODS
        #endregion
    }
}
