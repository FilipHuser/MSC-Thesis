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

namespace Graphium.ViewModels
{
    /// <summary>
    /// Interaction logic for PreferencesWindow.xaml
    /// </summary>
    public partial class PreferencesWindow : Window, IViewModelOwner
    {
        private readonly PreferencesWindowVM _vm;
        public PreferencesWindow()
        {
            InitializeComponent();
            _vm = new PreferencesWindowVM(this);
            DataContext = _vm;
        }

        public ViewModelBase GetViewModel() => _vm;
    }
}
