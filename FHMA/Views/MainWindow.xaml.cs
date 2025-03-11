using System.Windows;
using FHMA.ViewModels;
using FHMA.Views;

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
            var gcw = new GraphConfigWindow(this);

            gcw.OnGraphAdded += (graph) => { _mv.Graphs.Add(graph); };

            gcw.Owner = this;
            gcw.ShowDialog();
        }

        private void Button_StartCapturing(object sender, RoutedEventArgs e)
        {
            var mw = new MonitorWindow(_mv.Graphs);
            mw.Show();
            this.Close();
        }
    }
}