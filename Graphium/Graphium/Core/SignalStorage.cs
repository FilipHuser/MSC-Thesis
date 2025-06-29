using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Graphium.Models;

namespace Graphium.Core
{
    internal class SignalStorage
    {
        private string? _filePath;

        // Key: Signal, Value: latest value aligned by signal
        private Dictionary<Signal, object?> AlignedValues = new Dictionary<Signal, object?>();

        public delegate void AlignHandler();
        public event AlignHandler? OnAlignment;

        public SignalStorage(List<SignalBase> signals)
        {
            // Flatten all Signals from Signal and SignalComposite
            var allSignals = new List<Signal>();

            foreach (var signalBase in signals)
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

            // Initialize dictionary with signals and null values
            foreach (var signal in allSignals.Distinct())
            {
                AlignedValues[signal] = null;
            }

            OnAlignment += Flush;
            Init();
        }

        #region METHODS
        public void Add(Signal signal, object value)
        {
            if (!AlignedValues.ContainsKey(signal))
                throw new ArgumentException("Signal not found in storage.");

            AlignedValues[signal] = value;
            if (AlignedValues.All(x => x.Value != null))
            {
                OnAlignment?.Invoke();
                var keys = AlignedValues.Keys.ToList();
                foreach (var key in keys)
                    AlignedValues[key] = null;
            }
        }

        private void Init()
        {
            string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Graphium", "Measurements");
            if (!Directory.Exists(appDataPath)) Directory.CreateDirectory(appDataPath);
            _filePath = Path.Combine(appDataPath, "tmpMeasurement.csv");
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

            // Write timestamp followed by aligned values in signal order
            return $"{timestamp}{delimiter}" + string.Join(delimiter, AlignedValues.Values);
        }
        #endregion
    }
}
