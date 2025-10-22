using Graphium.Interfaces;
using NLog;
using NLog.Config;
using NLog.Targets;
using System.IO;

namespace Graphium.Services
{
    internal class LoggingService : ILoggingService
    {
        #region PROPERTIES
        private readonly Logger _logger;
        #endregion
        #region METHODS
        public LoggingService()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string logFolder = Path.Combine(appData, "Graphium", "Logs");

            if (!Directory.Exists(logFolder))
                Directory.CreateDirectory(logFolder);

            string logFilePath = Path.Combine(logFolder, "app.log");

            var config = new LoggingConfiguration();

            var fileTarget = new FileTarget("logfile")
            {
                FileName = logFilePath,
                Layout = "${longdate} | ${level:uppercase=true} | ${message} ${exception:format=toString}"
            };

            config.AddTarget(fileTarget);
            config.AddRule(NLog.LogLevel.Debug, NLog.LogLevel.Fatal, fileTarget);

            LogManager.Configuration = config;

            _logger = LogManager.GetCurrentClassLogger();
        }
        public void LogInfo(string message) => _logger.Info(message);
        public void LogDebug(string message) => _logger.Debug(message);
        public void LogWarning(string message) => _logger.Warn(message);
        public void LogError(string message) => _logger.Error(message);
        #endregion
    }
}
