using DataHub.Core;
using DataHub.Interfaces;

namespace DataHub
{
    public class Hub : IDisposable
    {
        #region PROPERITES
        public Dictionary<ModuleType, IModule> Modules { get; } = new();
        public bool IsCapturing { get; private set; }

        public event EventHandler<DataAvailableEventArgs>? DataAvailable;
        #endregion
        #region METHODS
        public void AddModule(IModule module)
        {
            Modules[module.ModuleType] = module;
            module.DataAvailable += OnModuleDataAvailable;
        }
        public void RemoveModule(IModule module)
        {
            module.DataAvailable -= OnModuleDataAvailable;
            Modules.Remove(module.ModuleType);
        }
        public ModuleBase<T>? GetModule<T>(ModuleType type)
        {
            return Modules.TryGetValue(type, out var module)
                ? module as ModuleBase<T>
                : null;
        }
        public void StartCapturing()
        {
            foreach (var module in Modules.Values)
                module.StartCapturing();
            IsCapturing = true;
        }
        public void StopCapturing()
        {
            foreach (var module in Modules.Values)
                module.StopCapturing();
            IsCapturing = false;
        }
        private void OnModuleDataAvailable(object? sender, DataAvailableEventArgs e)
        {
            DataAvailable?.Invoke(sender, e);
        }
        public void Dispose()
        {
            StopCapturing();
            foreach (var module in Modules.Values)
                module.Dispose();
            Modules.Clear();
        }
        #endregion
    }
}