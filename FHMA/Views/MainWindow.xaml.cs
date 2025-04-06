using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Serialization;
using FHMA.Core;
using FHMA.Models;
using FHMA.ViewModels;
using FHMA.Views;

namespace FHMA
{
    public partial class MainWindow : Window
    {
        private readonly MainWindowViewModel _vm;
         public MainWindow()
        {
            InitializeComponent();
            _vm = new MainWindowViewModel(this);
            DataContext = _vm;
        }
    }
}