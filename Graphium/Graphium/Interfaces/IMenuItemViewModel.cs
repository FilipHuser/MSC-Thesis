using System.Windows;
using System.Windows.Controls;

namespace Graphium.Interfaces
{
    public interface IMenuItemViewModel
    {
        public string Header { get; }
        public  UserControl Content { get; }
    }
}
