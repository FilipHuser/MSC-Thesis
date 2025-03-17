using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Xml.Serialization;
using FHMA.Models;
using FHMA.ViewModels;
using FHMA.Views;
using LiveChartsCore.SkiaSharpView;

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
            _mv.Graphs.ToList().ForEach(x => x.YAxes = [new Axis() { MinLimit = x.LowerBound, MaxLimit = x.UpperBound }]);
            var mw = new MonitorWindow(_mv.Graphs);
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
                serializer.Serialize(sw, _mv.Graphs.ToList());
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
                if (graphs != null) { _mv.Graphs = new ObservableCollection<Graph>(graphs); }

            }
        }

        private void Button_RemoveGraph(object sender, RoutedEventArgs e)
        {

        }
    }
}