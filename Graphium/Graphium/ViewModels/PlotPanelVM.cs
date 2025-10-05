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
    internal class PlotPanelVM : ViewModelBase
    {
        #region PROPERTIES
        private readonly Signal _signal;
        private int _height = 250;
        public int Height { get => _height; set { SetProperty(ref _height, value); } }  
        public string Name => _signal.ToString();
        public WpfPlot PlotControl => _signal.PlotControl;
        #region RELAY_COMMANDS
        public RelayCommand ResizeCmd => new RelayCommand(execute => {
            if (execute is System.Windows.Controls.Primitives.DragDeltaEventArgs args)
            {
                OnResize(args.VerticalChange);
            }
        });
        #endregion  
        #endregion
        public PlotPanelVM(Window window , Signal signal) : base(window)
        {
            _signal = signal;
            PlotControl.Plot.Layout.Frameless();
        }
        #region METHODS
        private void OnResize(double deltaY)
        {
            Height = Math.Max(50, Height + (int)deltaY);
        }
        #endregion
    }
}
