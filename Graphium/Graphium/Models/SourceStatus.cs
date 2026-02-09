using DataHub.Core;
using System.Diagnostics;

namespace Graphium.Models
{
    internal class SourceStatus : ModelBase
    {
        #region PROPERTIES
        private bool _isActive;
        private const int TimeoutMs = 500;
        private readonly Stopwatch _lastDataTimer = new();
        public ModuleType Type { get; set; }
        public bool IsActive { get => _isActive; set => SetProperty(ref _isActive, value); }
        #endregion
        #region METHODS
        public void MarkDataReceived()
        {
            _lastDataTimer.Restart();
            IsActive = true;
        }

        public void UpdateStatus()
        {
            if (_lastDataTimer.IsRunning && _lastDataTimer.ElapsedMilliseconds > TimeoutMs)
            {
                IsActive = false;
                _lastDataTimer.Stop();
            }
        }
        #endregion
    }
}
