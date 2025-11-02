using System.IO;
using Graphium.Models;
using Graphium.ViewModels;
using System.Globalization;

namespace Graphium.Core
{
    internal class SignalStorage
    {
        private string? _filePath;
        private Dictionary<Signal, object?> AlignedValues = new Dictionary<Signal, object?>();
        public SignalStorage(string measurementName, List<Signal> signals)
        {
            var allSignals = new List<Signal>();

            allSignals.AddRange(signals.Where(x => x.IsAcquired));

            foreach (var signal in allSignals.Distinct())
            {
                AlignedValues[signal] = null;
            }
            OnAlignment += Flush;
            Init(measurementName);
        }
        public delegate void AlignHandler();
        public event AlignHandler? OnAlignment;
        #region METHODS
        public void Add(Signal signal, object value)
        {
            if (!AlignedValues.ContainsKey(signal)) { 
                throw new ArgumentException("Signal not found in storage."); }

            AlignedValues[signal] = value;
            if (AlignedValues.All(x => x.Value != null))
            {
                OnAlignment?.Invoke();
                var keys = AlignedValues.Keys.ToList();
                foreach (var key in keys)
                    AlignedValues[key] = null;
            }
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
        public string ToCsv(char delimiter)
        {
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var valuesAsString = AlignedValues.Values.Select(v =>
            {
                if (v == null) return "";
                if (v is List<object> list)
                {
                    return string.Join(',', list.Select(x =>
                    {
                        if (x == null) return "";
                        if (x is IFormattable f) return f.ToString(null, CultureInfo.InvariantCulture);
                        return x.ToString() ?? "";
                    }));
                }

                if (v is IFormattable formattable) return formattable.ToString(null, CultureInfo.InvariantCulture);

                return v.ToString() ?? "";
            });

            return $"{timestamp}{delimiter}" + string.Join(delimiter, valuesAsString);
        }
        #endregion
    }
}