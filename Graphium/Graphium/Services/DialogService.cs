using System;
using System.IO;
using System.Windows;
using Graphium.Interfaces;
using Microsoft.Win32;

namespace Graphium.Services
{
    internal class DialogService : IDialogService
    {
        #region METHODS
        public void ShowInfo(string message, string title = "Information")
        {
            MessageBox.Show(
                message,
                title,
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        }
        public void ShowWarning(string message, string title = "Warning")
        {
            MessageBox.Show(
                message,
                title,
                MessageBoxButton.OK,
                MessageBoxImage.Warning
            );
        }
        public void ShowError(string message, string title = "Error")
        {
            MessageBox.Show(
                message,
                title,
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
        }
        public bool ShowConfirmation(string message, string title = "Confirm")
        {
            var result = MessageBox.Show(
                message,
                title,
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            return result == MessageBoxResult.Yes;
        }
        public string? ShowSaveFile(string filter, string defaultFileName, string? initialDirectory = null)
        {
            var dialog = new SaveFileDialog
            {
                Filter = filter,
                FileName = defaultFileName,
                InitialDirectory = initialDirectory ?? Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments))
            };

            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }
        #endregion
    }
}
