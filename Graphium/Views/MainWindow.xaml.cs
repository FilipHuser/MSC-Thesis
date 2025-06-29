using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Serialization;
using Graphium.Core;
using Graphium.Models;
using Graphium.ViewModels;
using Graphium.Views;

namespace Graphium
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