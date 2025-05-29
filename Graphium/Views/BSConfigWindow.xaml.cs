using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Text.RegularExpressions;
using Graphium.Models;
using Graphium.ViewModels;

namespace Graphium.Views
{
    public partial class BSConfigWindow : Window
    {
        private readonly BSConfigWindowViewModel _vm;
        public delegate void GraphAddHandler(BiologicalSignal BiologicalSignal);
        public event GraphAddHandler? OnGraphAdded;
        public BSConfigWindow()
        {
            InitializeComponent();
            _vm = new BSConfigWindowViewModel(this);
            DataContext = _vm;
        }
        public void RaiseOnGraphAdded(BiologicalSignal bs)
        { 
            OnGraphAdded?.Invoke(bs);
            Close();
        }
        private void TextBox_NumberValidation(object sender, TextCompositionEventArgs e)
        {
            var textBox = (TextBox)sender;
            var newText = textBox.Text.Insert(textBox.CaretIndex, e.Text);
            e.Handled = !Regex.IsMatch(newText, @"^-?[0-9]*$");
        }
    }
}
