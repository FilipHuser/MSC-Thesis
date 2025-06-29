using System.Collections.ObjectModel;
using System.Windows;
using Graphium.Models;
using Graphium.ViewModels;

namespace Graphium.Views
{
    /// <summary>
    /// Interaction logic for MonitorWindow.xaml
    /// </summary>
    public partial class MonitorWindow : Window
    {
        #region PROPERTIES
        private readonly MonitorViewModel _vm;
        #endregion
        #region METHODS
        public MonitorWindow(ObservableCollection<SignalBase> signals)
        {
            InitializeComponent();
            _vm = new MonitorViewModel(this , signals);
            DataContext = _vm;
        }
        #endregion
    }
}
