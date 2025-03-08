using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FHMA.ViewModels;

namespace FHMA
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainWindowViewModel _mv;
        public MainWindow()
        {
            InitializeComponent();
            _mv = new MainWindowViewModel();
            DataContext = _mv;
        }

        private void Button_AddGraph(object sender, RoutedEventArgs e)
        {
            _mv.Graphs.Add(new Models.Graph());
        }
    }
}