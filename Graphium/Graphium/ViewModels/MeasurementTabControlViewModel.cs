using System.Windows;
using System.Windows.Controls;
using Graphium.Controls;

namespace Graphium.ViewModels
{
    class MeasurementTabControlViewModel : BaseViewModel
    {
        #region PROPERTIES
        public string Title { get; set; }
        public UserControl Tab { get; set; }
        #endregion
        #region METHODS
        public MeasurementTabControlViewModel(Window window , string title) : base(window)
        {
            Title = title;
            Tab = new MeasurementTabControl(window, title);
        }
        #endregion
    }
}
