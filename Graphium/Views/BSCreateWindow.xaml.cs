using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Graphium.ViewModels;
using Graphium.Models;
using Graphium.Core;

namespace Graphium.Views
{
    public partial class BSCreateWindow : Window
    {
        private readonly BSCreateWindowViewModel _vm;
        public BSCreateWindow()
        {
            InitializeComponent();
            _vm = new BSCreateWindowViewModel(this);
            DataContext = _vm;
        }
        private void TextBox_NumberValidation(object sender, TextCompositionEventArgs e) 
        {
            var textBox = (TextBox)sender;
            var newText = textBox.Text.Insert(textBox.CaretIndex, e.Text);
            e.Handled = !Regex.IsMatch(newText, @"^-?[0-9]*$");
        }
    }
}
