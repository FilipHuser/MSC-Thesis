using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graphium.Interfaces
{
    internal interface IFileExportService
    {
        #region METHODS
        Task<bool> ExportAsync(string sourceFilePath, string defaultFileName, string filter);
        #endregion
    }
}
