using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graphium.Interfaces
{
    interface IDialogService
    {
        void ShowInfo(string message, string title = "Information");
        void ShowWarning(string message, string title = "Warning");
        void ShowError(string message, string title = "Error");
        bool ShowConfirmation(string message, string title = "Confirm");
    }
}
