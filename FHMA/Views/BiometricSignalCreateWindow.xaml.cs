using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using FHMA.ViewModels;
using FHMA.Models;
using FHMA.Core;

namespace FHMA.Views
{
    public partial class BiometricSignalCreateWindow : Window
    {
        private readonly BiometricSignalCreateWindowViewModel _vm;
        public BiometricSignalCreateWindow()
        {
            InitializeComponent();
            _vm = new BiometricSignalCreateWindowViewModel();
            DataContext = _vm;
        }

        private void Button_SaveBiometricSignal(object sender, RoutedEventArgs e)
        {
            var fileName = $"{_vm.BiometricSignal}.xml";

            XmlSettingsManager.StoreSettings("BiometricSignals" , fileName , _vm.BiometricSignal);
            this.Close();
        }
        private void Button_ClearAllGraphs(object sender, RoutedEventArgs e) => _vm.Graphs.Clear();

        private void Button_AddGraph(object sender, RoutedEventArgs e)
        {
            _vm.Graphs.Add((Graph)_vm.Graph.Clone());
            _vm.Graph = new Models.Graph();
        }

        private void TextBox_NumberValidation(object sender, TextCompositionEventArgs e) 
        {
            var textBox = (TextBox)sender;
            var newText = textBox.Text.Insert(textBox.CaretIndex, e.Text);
            e.Handled = !Regex.IsMatch(newText, @"^-?[0-9]*$");
        }
    }
}
