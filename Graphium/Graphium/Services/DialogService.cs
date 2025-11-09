using System;
using System.Windows;
using Graphium.Interfaces;

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
        #endregion
    }
}
