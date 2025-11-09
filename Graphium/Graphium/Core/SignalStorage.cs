using System.IO;
using Graphium.Models;
using Graphium.ViewModels;
using System.Globalization;

namespace Graphium.Core
{
    internal class SignalStorage
    {
        #region PROPERTIES
        private string? _filePath;
        private Dictionary<Signal, (object? value, DateTime? timestamp)> AlignedValues = [];
        public SignalStorage(string measurementName, List<Signal> signals)
        {
            var allSignals = new List<Signal>();

            allSignals.AddRange(signals.Where(x => x.IsAcquired));

            foreach (var signal in allSignals.Distinct())
            {
                AlignedValues[signal] = (null,null);
            }
            OnAlignment += Flush;
            Init(measurementName);
        }
        public delegate void AlignHandler();
        public event AlignHandler? OnAlignment;
        #endregion
        #region METHODS
        public void Add(Signal signal, object value, DateTime timestamp)
        {
            if (!AlignedValues.ContainsKey(signal)) { throw new ArgumentException("Signal not found in storage."); }

            AlignedValues[signal] = (value, timestamp);
            if (AlignedValues.All(x => x.Value.value != null && x.Value.timestamp != null))
            {
                OnAlignment?.Invoke();
                var keys = AlignedValues.Keys.ToList();
                foreach (var key in keys)
                    AlignedValues[key] = (null, null);
            }
        }
        public string ToCsv(char delimiter)
        {
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var valuesAsString = AlignedValues.Values.Select(v =>
            {
                if (v.value == null) return "";
                if (v.value is List<object> list)
                {
                    return string.Join(',', list.Select(x =>
                    {
                        if (x == null) return "";
                        if (x is IFormattable f) return f.ToString(null, CultureInfo.InvariantCulture);
                        return x.ToString() ?? "";
                    }));
                }

                if (v.value is IFormattable formattable) return formattable.ToString(null, CultureInfo.InvariantCulture);

                return v.ToString() ?? "";
            });

            return $"{timestamp}{delimiter}" + string.Join(delimiter, valuesAsString);
        }
        private void Init(string fileName)
        {
            string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Graphium", "Measurements");
            if (!Directory.Exists(appDataPath)) Directory.CreateDirectory(appDataPath);
            _filePath = Path.Combine(appDataPath, $"{fileName}_tmpMeasurement.csv");
            if (File.Exists(_filePath)) File.WriteAllText(_filePath, string.Empty);
        }
        private void Flush()
        {
            string data = ToCsv(';');
            using (var sw = new StreamWriter(_filePath!, append: true))
            {
                sw.WriteLine(data);
            }
        }
        #endregion
    }
}