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
        public ObservableCollection<Graph> Graphs { get; set; } = new ObservableCollection<Graph>();
        public string GraphCount => $"{Graphs.Count} / {ConfigurationManager.AppSettings["MaxGraphs"]}";
        public MainWindowViewModel()
        {
            Graphs.CollectionChanged += (object? sender, NotifyCollectionChangedEventArgs e) => { OnPropertyChanged(nameof(GraphCount)); };
        }
    }
}
