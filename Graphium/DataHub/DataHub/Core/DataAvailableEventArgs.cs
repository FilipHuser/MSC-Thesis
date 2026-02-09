namespace DataHub.Core
{
    public class DataAvailableEventArgs
    {
        #region PROPERTIES
        public ModuleType Source { get; }
        #endregion
        #region METHODS
        public DataAvailableEventArgs(ModuleType source) => Source = source;
        #endregion
    }
}
