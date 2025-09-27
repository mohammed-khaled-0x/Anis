using System;
using System.Globalization;
using System.Windows.Data;

namespace Anis.App.Converters;

public class ObjectEqualityConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        // We are comparing two objects. Return true if they are the same instance.
        if (values.Length < 2)
        {
            return false;
        }

        return ReferenceEquals(values[0], values[1]);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}