using System.Windows;
using Graphium.ViewModels;

namespace Graphium
{
    public partial class MainWindow : Window
    {
         public MainWindow(object dataContext)
        {
            InitializeComponent();
            DataContext = dataContext;
        }
    }
}