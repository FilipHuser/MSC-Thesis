using System.Globalization;
using System.Windows.Data;

namespace Graphium.Converters
{
    class TypeConverter : IValueConverter
    {
        #region METHODS
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value?.GetType().Name ?? string.Empty;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
        #endregion
    }
}
