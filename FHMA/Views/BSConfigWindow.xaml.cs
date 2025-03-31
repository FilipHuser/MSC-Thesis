using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Text.RegularExpressions;
using FHMA.Models;
using FHMA.ViewModels;

namespace FHMA.Views
{
    public partial class BSConfigWindow : Window
    {
        private readonly BSConfigWindowViewModel _vm;
        public delegate void GraphAddHandler(BiometricSignal biometricSignal);
        public event GraphAddHandler? OnGraphAdded;
        public BSConfigWindow()
        {
            InitializeComponent();
            _vm = new BSConfigWindowViewModel(this);
            DataContext = _vm;
        }

        private void Button_AddBiometricSignal(object sender, RoutedEventArgs e)
        {
            if (_vm.BiometricSignal == null) { return; }
            OnGraphAdded?.Invoke(_vm.BiometricSignal);
            this.Close();
        }
        private void Button_CreateBiometricSignal(object sender, RoutedEventArgs e)
        {
            var bscw = new BSCreateWindow();
            bscw.Owner = this;

            bscw.Closed += (s, e) => { _vm.Refresh(); };
            bscw.Show();
        }

        private void TextBox_NumberValidation(object sender, TextCompositionEventArgs e)
        {
            var textBox = (TextBox)sender;
            var newText = textBox.Text.Insert(textBox.CaretIndex, e.Text);
            e.Handled = !Regex.IsMatch(newText, @"^-?[0-9]*$");
        }
    }
}
