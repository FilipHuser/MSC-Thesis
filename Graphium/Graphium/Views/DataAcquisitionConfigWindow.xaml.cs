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
using Graphium.Interfaces;
using Graphium.ViewModels;

namespace Graphium.Views
{
    partial class DataAcquisitionConfigWindow : Window, IViewModelOwner
    {
        private readonly DataAcquisitionConfigWindowVM _vm;
        public DataAcquisitionConfigWindow()
        {
            InitializeComponent();
            _vm = new DataAcquisitionConfigWindowVM(this);
            DataContext = _vm;
        }

        public ViewModelBase GetViewModel() => _vm;
    }
}
