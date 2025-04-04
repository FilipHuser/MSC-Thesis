using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FHMA.Models;

namespace FHMA.Core
{
    class BiometricSignalStorage
    {
        private string? _filePath;
        private  Dictionary<Graph, object?> AlignedValues = new Dictionary<Graph, object?>();
        public delegate void AlignHandler();
        public event AlignHandler OnAlignment;
        public BiometricSignalStorage(List<BiometricSignal> biometricSignals)
        {
            biometricSignals.SelectMany(x => x.Graphs).ToList().ForEach(x => AlignedValues[x] = null);
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
            string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FHMA", "Measurements");
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
