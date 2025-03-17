using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
            OnGraphAdded?.Invoke(_vm.Graph);
            this.Close();
        }

        private void TextBox_NumberValidation(object sender, TextCompositionEventArgs e)
        {
            var textBox = (TextBox)sender;
            var newText = textBox.Text.Insert(textBox.CaretIndex, e.Text);
            e.Handled = !Regex.IsMatch(newText, @"^-?[0-9]*$");
        }
    }
}
