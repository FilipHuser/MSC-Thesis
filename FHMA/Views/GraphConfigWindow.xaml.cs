using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using FHMA.Models;
using FHMA.ViewModels;

namespace FHMA.Views
{
    /// <summary>
    /// Interaction logic for GraphConfigWindow.xaml
    /// </summary>
    public partial class GraphConfigWindow : Window
    {
        private readonly GraphConfigWindowViewModel _vm;
        public delegate void GraphAddHandler(Graph graph);
        public event GraphAddHandler? OnGraphAdded;
        public GraphConfigWindow(Window parent)
        {
            InitializeComponent();
            parent.Closed += (s, e) => this.Close();
            _vm = new GraphConfigWindowViewModel();
            DataContext = _vm;
        }

        private void Button_AddGraph(object sender, RoutedEventArgs e)
        {
            var graph = new Graph()
            {
                Channel = _vm.Channel ?? throw new ArgumentNullException(),
                ModuleType = _vm.ModuleType ?? throw new ArgumentNullException(),
            };
            OnGraphAdded?.Invoke(graph);
            this.Close();
        }
    }
}
