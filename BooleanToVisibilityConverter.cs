using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Library.App.Converters;

/// <summary>bool → Visibility; параметр "Invert" меняет поведение на обратное.</summary>
public sealed class BooleanToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var flag = value is bool result && result;
        if (parameter is string text && text.Equals("Invert", StringComparison.OrdinalIgnoreCase)) flag = !flag;
        return flag ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is Visibility visibility && visibility == Visibility.Visible;
}