using System.Globalization;
using System.Windows.Data;

namespace Library.App.Converters;

/// <summary>Инверсия bool: используется для IsEnabled во время занятости.</summary>
public sealed class InverseBooleanConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool flag ? !flag : true;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool flag ? !flag : false;
}