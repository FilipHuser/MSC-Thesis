using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Graphium.Core;
using Graphium.Models;
using ScottPlot.WPF;

namespace Graphium.ViewModels
{
    internal class PlotPanelControlVM : ViewModelBase
    {
        #region PROPERTIES
        private int _height = 250;
        private string _plotMax = "";
        private string _plotMin = "";
        private readonly Signal _signal;
        public string Name => _signal.ToString();
        public int Height { get => _height; set { SetProperty(ref _height, value); } }  
        public WpfPlot PlotControl => _signal.PlotControl;
        public string PlotMax
        {
            get => _plotMax;
            private set => SetProperty(ref _plotMax, value);
        }
        public string PlotMin
        {
            get => _plotMin;
            private set => SetProperty(ref _plotMin, value);
        }

        #region RELAY_COMMANDS
        public RelayCommand ResizeCmd => new RelayCommand(execute => {
            if (execute is System.Windows.Controls.Primitives.DragDeltaEventArgs args)
            {
                OnResize(args.VerticalChange);
            }
        });
        #endregion  
        #endregion
        public PlotPanelControlVM(Window window , Signal signal) : base(window)
        {
            _signal = signal;
            PlotControl.Plot.Layout.Frameless();

            PlotControl.Plot.RenderManager.RenderStarting += (s, e) =>
            {
                var yLimits = PlotControl.Plot.Axes.GetLimits().YRange;
                PlotMax = yLimits.Max.ToString("0.00");
                PlotMin = yLimits.Min.ToString("0.00");
            };
        }
        #region METHODS
        private void OnResize(double deltaY)
        {
            Height = Math.Max(50, Height + (int)deltaY);
        }
        #endregion
    }
}
