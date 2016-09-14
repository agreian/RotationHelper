using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace RotationHelper.View
{
    internal class EnumerableNullReplaceConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var collection = (IEnumerable)value;

            return collection.Cast<object>().Select(x => x ?? parameter).ToArray();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}