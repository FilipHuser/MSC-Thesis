using System.IO;
using Graphium.Interfaces;

namespace Graphium.Services
{
    internal class FileExportService : IFileExportService
    {
        #region PROPERTIES
        private readonly IDialogService _dialogService;
        #endregion
        #region METHODS
        public FileExportService(IDialogService dialogService)
        {
            _dialogService = dialogService;
        }
        public async Task<bool> ExportAsync(string sourceFilePath, string defaultFileName, string filter = "All files (*.*)|*.*")
        {
            if (!File.Exists(sourceFilePath))
            {
                _dialogService.ShowWarning("Source file not found.");
                return false;
            }

            var targetPath = _dialogService.ShowSaveFile(filter, defaultFileName);
            if (string.IsNullOrEmpty(targetPath))
                return false;

            try
            {
                await Task.Run(() => File.Copy(sourceFilePath, targetPath, true));
                _dialogService.ShowInfo("File exported successfully.");
                return true;
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Export failed: {ex.Message}");
                return false;
            }
        }
        #endregion
    }
}
