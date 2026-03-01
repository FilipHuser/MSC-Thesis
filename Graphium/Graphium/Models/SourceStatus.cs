using DataHub.Core;
using System.Diagnostics;

namespace Graphium.Models
{
    internal class SourceStatus : ModelBase
    {
        #region PROPERTIES
        private bool _isActive;
        private readonly int _timeoutMs;
        private readonly Stopwatch _lastDataTimer = new();
        public ModuleType Type { get; set; }
        public bool IsActive { get => _isActive; set => SetProperty(ref _isActive, value); }
        #endregion
        #region METHODS
        public SourceStatus(int timeoutMs)
        {
            _timeoutMs = timeoutMs;
        }
        public void MarkDataReceived()
        {
            _lastDataTimer.Restart();
            IsActive = true;
        }
        public void UpdateStatus()
        {
            if (_lastDataTimer.IsRunning && _lastDataTimer.ElapsedMilliseconds > _timeoutMs)
            {
                IsActive = false;
                _lastDataTimer.Stop();
            }
        }
        #endregion
    }
}
