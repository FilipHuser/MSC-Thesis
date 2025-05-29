using System.IO;
using Graphium.Models;

namespace Graphium.Core
{
    internal class BiologicalSignalStorage
    {
        private string? _filePath;
        private  Dictionary<Graph, object?> AlignedValues = new Dictionary<Graph, object?>();
        public delegate void AlignHandler();
        public event AlignHandler OnAlignment;
        public BiologicalSignalStorage(List<BiologicalSignal> biologicalSignals)
        {
            biologicalSignals.SelectMany(x => x.Graphs).ToList().ForEach(x => AlignedValues[x] = null);
            OnAlignment += Flush;
            Init();
        }

        #region METHODS
        public void Add(Graph graph , object value)
        {
            AlignedValues[graph] = value;
            if (AlignedValues.All(x => x.Value != null))
            {
                OnAlignment?.Invoke();
            }
        }
        private void Init()
        {
            string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Graphium", "Measurements");
            if (!Directory.Exists(appDataPath)) { Directory.CreateDirectory(appDataPath); }
            _filePath = Path.Combine(appDataPath, "tmpMesurement.csv");
            if (File.Exists(_filePath)) { File.WriteAllText(_filePath, string.Empty); }
        }
        private void Flush()
        {
            string data = ToCsv(';');

            using (var sw = new StreamWriter(_filePath!, append:true))
            {
                sw.WriteLine(data);
            }
        }
        public string ToCsv(char delimiter)
        {
            var timeStamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            return $"{timeStamp};" + string.Join(delimiter, AlignedValues.Values);
        }
        #endregion
    }
}
