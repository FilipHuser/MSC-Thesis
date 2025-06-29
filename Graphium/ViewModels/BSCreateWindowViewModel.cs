using System.Collections.ObjectModel;
using System.Configuration;
using System.Windows;
using Graphium.Core;
using Graphium.Models;

namespace Graphium.ViewModels
{
    #region AVAILABLE_ITEMS

    #endregion
    internal class BSCreateWindowViewModel : BaseViewModel
    {
        #region PROPERTIES
        private Graph _graph = new Graph();
        //public List<PacketSource> Sources { get; }
        //public BiologicalSignal BiologicalSignal { get; set; } = new BiologicalSignal();
        public ObservableCollection<Graph> Graphs { get; set; } = new ObservableCollection<Graph>();
        public Graph Graph { get => _graph; set => SetProperty(ref _graph, value); }
        #endregion
        #region RELAY_COMMANDS
        public RelayCommand AddGraphCommand => new RelayCommand(execute => AddGraph());
        public RelayCommand CreateBSCommand => new RelayCommand(execute => CreateBiologicalSignal() , canExecute => Graphs.Count > 0);
        public RelayCommand ClearGraphsCommand => new RelayCommand(execute => ClearGraphs() , canExecute => Graphs.Count > 0);
        #endregion
        public BSCreateWindowViewModel(Window window) : base(window)
        {
            //Sources = Enum.GetValues(typeof(PacketSource)).Cast<PacketSource>().ToList();
            //BiologicalSignal.Source = Sources.First();
        }
        #region METHODS
        private void AddGraph()
        {
            Graphs.Add((Graph)Graph.Clone());
            Graph = new();
        }
        private void CreateBiologicalSignal()
        {
            BiologicalSignal.Graphs = Graphs.ToList();
            XmlManager.Store("BiologicalSignals", $"{BiologicalSignal}.xml", BiologicalSignal);
            Window.Close();
        }
        private void ClearGraphs() => Graphs.Clear();
        #endregion
    }
}
