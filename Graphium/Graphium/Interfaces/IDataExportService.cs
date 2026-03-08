using Graphium.Models;

namespace Graphium.Interfaces
{
    internal interface IDataExportService
    {
        bool IsEnabled { get; set; }
        void Start();
        void Stop();
        void Export(Dictionary<SignalBase, object> rowValues, double timestamp);
    }
}