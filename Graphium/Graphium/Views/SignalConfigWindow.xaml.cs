using System.Windows;
using Graphium.Models;
using Graphium.ViewModels;

namespace Graphium.Views
{
    /// <summary>
    /// Interaction logic for SignalConfigWindow.xaml
    /// </summary>
    public partial class SignalConfigWindow : Window
    {
        #region PROPERTIES
        private readonly SignalConfigViewModel _vm;
        public delegate void SignalAddHandler(SignalBase signal);
        public event SignalAddHandler? OnSignalAdded;
        #endregion
        #region METHODS
        public SignalConfigWindow()
        {
            InitializeComponent();
            _vm = new SignalConfigViewModel(this);
            DataContext = _vm;
        }
        public void RaiseOnGrapAdded(SignalBase signal)
        {
            OnSignalAdded?.Invoke(signal);
            Close();
        }
        #endregion
    }
}
