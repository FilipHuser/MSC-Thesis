using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Graphium.ViewModels;

namespace Graphium.Views
{
    /// <summary>
    /// Interaction logic for SignalCreatorWindow.xaml
    /// </summary>
    public partial class SignalCreatorWindow : Window
    {
        public SignalCreatorWindow()
        {
            InitializeComponent();
            Closing += OnWindowClosing;
        }
        private void OnWindowClosing(object? sender, CancelEventArgs e)
        {
            if (DataContext is SignalCreatorViewModel viewModel)
            {
                viewModel.OnSignalClosing();
            }
        }
    }
}
