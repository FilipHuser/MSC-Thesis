using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using FHAPILib;
using FHMA.Core;
using FHMA.Models;
using FHMA.ViewModels;
using ScottPlot.WPF;
using SkiaSharp;

namespace FHMA.Views
{
    /// <summary>
    /// Interaction logic for MonitorWindow.xaml
    /// </summary>
    public partial class MonitorWindow : Window
    {
        private readonly MonitorWindowViewModel _vm;

        public MonitorWindow(ObservableCollection<BiometricSignal> biometricSignals)
        {
            InitializeComponent();
            _vm = new MonitorWindowViewModel(this , biometricSignals);
            DataContext = _vm;
        }
    }
}
