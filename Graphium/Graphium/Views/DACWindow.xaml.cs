using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using Graphium.Interfaces;
using Graphium.Models;
using Graphium.ViewModels;

namespace Graphium.Views
{
    //DATA ACQUISITION CONFIG
    partial class DACWindow : Window, IViewModelOwner
    {
        private readonly DACWindowVM _vm;
        public DACWindow(ObservableCollection<SignalBase> signals)
        {
            InitializeComponent();
            _vm = new DACWindowVM(this , signals);
            DataContext = _vm;
        }

        public ViewModelBase GetViewModel() => _vm;
    }
}
