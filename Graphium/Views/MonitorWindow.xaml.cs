using System.Collections.ObjectModel;
using Graphium.ViewModels;
using Graphium.Models;
using System.Windows;

namespace Graphium.Views
{
    /// <summary>
    /// Interaction logic for MonitorWindow.xaml
    /// </summary>
    public partial class MonitorWindow : Window
    {
        private readonly MonitorWindowViewModel _vm;

        public MonitorWindow(ObservableCollection<BiologicalSignal> biologicalSignals)
        {
            InitializeComponent();
            _vm = new MonitorWindowViewModel(this , biologicalSignals);
            DataContext = _vm;
        }
    }
}
