using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Serialization;
using FHMA.Models;
using FHMA.ViewModels;
using FHMA.Views;

namespace FHMA
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainWindowViewModel _vm;
        public MainWindow()
        {
            InitializeComponent();
            _vm = new MainWindowViewModel();
            DataContext = _vm;
        }

        private void Button_AddGraph(object sender, RoutedEventArgs e)
        {
            var gcw = new GraphConfigWindow(this);
            gcw.OnGraphAdded += (graph) => { _vm.Graphs.Add(graph); };
            gcw.Owner = this;
            gcw.ShowDialog();
        }

        private void Button_StartCapturing(object sender, RoutedEventArgs e)
        {
            if (_vm.Graphs.Count == 0)
            {
                MessageBox.Show("You need to add at least one graph before proceeding.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var mw = new MonitorWindow(_vm.Graphs);
            mw.Show();
            this.Close();
        }
        private void Button_SaveTemplate(object sender, RoutedEventArgs e)
        {
            string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FHMA", "Config");
             
            if (!Directory.Exists(appDataPath)){ Directory.CreateDirectory(appDataPath); }

            string filePath = Path.Combine(appDataPath , "GraphsTemplate.xml");

            XmlSerializer serializer = new XmlSerializer(typeof(List<Graph>));

            using (var sw = new StreamWriter(filePath))
            {
                serializer.Serialize(sw, _vm.Graphs.ToList());
            }
        }
        private void Button_LoadTemplate(object sender, RoutedEventArgs e)
        {
            string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FHMA", "Config");

            if (!Directory.Exists(appDataPath)) { Directory.CreateDirectory(appDataPath); }

            string filePath = Path.Combine(appDataPath, "GraphsTemplate.xml");

            XmlSerializer serializer = new XmlSerializer(typeof(List<Graph>));

            using (var sr = new StreamReader(filePath))
            {
                var graphs = (List<Graph>?)serializer.Deserialize(sr);
                if (graphs != null)
                {
                    _vm.Graphs.Clear();
                    foreach (var graph in graphs)
                    {
                        _vm.Graphs.Add(graph);
                    }
                }
            }
        }

        private void Button_RemoveGraph(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is Graph graph) { _vm.Graphs.Remove(graph); }
        }
    }
}