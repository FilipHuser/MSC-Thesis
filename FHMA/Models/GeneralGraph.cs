using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScottPlot.Plottables;
using ScottPlot.WPF;
using System.Xml.Serialization;

namespace FHMA.Models
{
    class GeneralGraph : Graph
    {
        public WpfPlot PlotControl { get; } = new WpfPlot();
        public DataStreamer Streamer { get; set; }

        public GeneralGraph()
        {
            PlotControl.UserInputProcessor.Disable();
            PlotControl.Plot.Axes.AutoScale();
            Streamer = PlotControl.Plot.Add.DataStreamer(PointLimit);
        }
    }
}
