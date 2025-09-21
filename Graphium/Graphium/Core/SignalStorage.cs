using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Graphium.Models;
using Graphium.ViewModels;

namespace Graphium.Core
{
    internal class SignalStorage
    {
        private string? _filePath;

        private Dictionary<Signal, object?> AlignedValues = new Dictionary<Signal, object?>();

        public delegate void AlignHandler();
        public event AlignHandler? OnAlignment;

        public SignalStorage(MTControlVM parent)
        {
            var allSignals = new List<Signal>();

            foreach (var signalBase in parent.Signals.Where(x => x.IsAcquired))
            {
                switch (signalBase)
                {
                    case Signal s:
                        allSignals.Add(s);
                        break;
                    case SignalComposite sc:
                        allSignals.AddRange(sc.Signals);
                        break;
                }
            }
            foreach (var signal in allSignals.Distinct())
            {
                AlignedValues[signal] = null;
            }
            OnAlignment += Flush;
            Init(parent.Title);
        }

        #region METHODS
        public void Add(Signal signal, object value)
        {
            if (!AlignedValues.ContainsKey(signal)) { throw new ArgumentException("Signal not found in storage."); }

            AlignedValues[signal] = value;
            if (AlignedValues.All(x => x.Value != null))
            {
                OnAlignment?.Invoke();
                var keys = AlignedValues.Keys.ToList();
                foreach (var key in keys)
                    AlignedValues[key] = null;
            }
        }
        public void Add(SignalComposite signalComposite, Dictionary<int, List<object>> values)
        {
            for (int i = 0; i < signalComposite.Signals.Count; i++)
            {
                if (values.TryGetValue(i, out var list) && list.Count > 0)
                {
                    Add(signalComposite.Signals[i], list.First());
                }
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
            return $"{timestamp}{delimiter}" + string.Join(delimiter, AlignedValues.Values);
        }
        #endregion
    }
}
