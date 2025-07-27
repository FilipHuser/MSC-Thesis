using System.Collections.ObjectModel;
using System.Reflection.Emit;
using System.Windows;
using System.Windows.Controls;
using DataHub;
using Graphium.Controls;
using Graphium.Models;
using ScottPlot;
using ScottPlot.WPF;

namespace Graphium.ViewModels
{
    public partial class MeasurementTabControlVM : ViewModelBase
    {
        #region PROPERTIES
        private readonly Hub _dh;
        private ObservableCollection<SignalBase> _signals = [];
        public string Title { get; set; }
        public UserControl Tab { get; set; }
        public WpfPlot Plot { get; set; } = new WpfPlot();
        public ObservableCollection<SignalBase> Signals { get => _signals; set { SetProperty(ref _signals, value); OnSignalsUpdate(); } }
        #endregion
        public MeasurementTabControlVM(Window parent , string title , ref Hub dh) : base(parent)
        {
            Plot.Multiplot.RemovePlot(Plot.Multiplot.GetPlot(0));

            _dh = dh;
            Title = title;
            Tab = new MeasurementTabControl(title);
            Signals.CollectionChanged += (s,e) => {
                OnSignalsUpdate(); 
            };
        }
        #region METHODS
        private void OnSignalsUpdate()
        {
            foreach(var signal in Signals)
            {
                switch (signal)
                {
                    case Signal s:
                        Plot.Multiplot.AddPlot(s.Plot);
                        break;
                    case SignalComposite sc:
                        sc.Plots.ForEach(x => Plot.Multiplot.AddPlot(x));
                        break;
                }
            }
            Plot.Multiplot.CollapseVertically();
            Plot.Multiplot.SharedAxes.ShareX(Plot.Multiplot.GetPlots());
            Plot.Refresh();
        }
        #endregion
    }
}
