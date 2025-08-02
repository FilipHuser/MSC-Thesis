using System.Data;
using System.IO;
using Graphium.Core;
using ScottPlot;
using ScottPlot.Plottables;

namespace Graphium.Models
{
    internal class Signal : SignalBase
    {
        #region PROPERTIES
        public override int Count => 1;
        public Graph Graph { get; set; }
        public Plot Plot { get; set; } = new Plot();
        public DataStreamer? Stream { get; set; }
        #endregion
        #region METHODS
        public Signal(Type source, Graph? graph = null) : base(source)
        {
            Graph = graph ?? new Graph();
            Plot.Axes.SetLimitsY(Graph.LowerBound , Graph.UpperBound);
            Stream = Plot.Add.DataStreamer(Graph.Capacity);
            Stream.LegendText = this.ToString();
        }
        public override void Update(Dictionary<int , List<object>> data)
        {
            var values = data.Single().Value;

            if (values.Count == 0 || values[0] is not IConvertible)
                return;

            Type valueType = values[0].GetType();
            double sourceMin, sourceMax;

            (sourceMin, sourceMax) = valueType switch
            {
                Type t when t == typeof(byte) => (byte.MinValue, byte.MaxValue),
                Type t when t == typeof(short) => (short.MinValue, short.MaxValue),
                Type t when t == typeof(int) => (int.MinValue, int.MaxValue),
                Type t when t == typeof(float) => (float.MinValue, float.MaxValue),
                Type t when t == typeof(double) => (double.MinValue, double.MaxValue),
                _ => throw new NotSupportedException($"Type {valueType.Name} not supported.")
            };
             
            double rangeIn = sourceMax - sourceMin;
            double rangeOut = Graph.UpperBound - Graph.LowerBound;

            Func<object, double> mapRange = v =>
            {
                double val = Convert.ToDouble(v);
                return Graph.LowerBound + ((val - sourceMin) / rangeIn) * rangeOut;
            };

            //Stream?.AddRange(values.Select(mapRange));
            Stream?.AddRange(data.Single().Value.Select(v => Convert.ToDouble(v)));
        }
        public override string ToString() => Graph.Label??"~";
        #endregion
    }
}
