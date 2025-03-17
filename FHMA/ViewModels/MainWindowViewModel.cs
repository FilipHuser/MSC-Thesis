using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.Runtime.CompilerServices;
using FHMA.Models;

namespace FHMA.ViewModels
{
    internal class MainWindowViewModel : BaseViewModel
    {
        private ObservableCollection<Graph> _graphs = new ObservableCollection<Graph>();
        public ObservableCollection<Graph> Graphs { get => _graphs; set => SetProperty(ref _graphs, value); }
        public string GraphCount => $"{Graphs.Count} / {ConfigurationManager.AppSettings["MaxChannels"]}";
        public MainWindowViewModel()
        {
            Graphs.CollectionChanged += (object? sender, NotifyCollectionChangedEventArgs e) => { OnPropertyChanged(nameof(GraphCount)); };
        }
    }
}
