using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Library.App.Converters;

/// <summary>Цвет по числу дней просрочки: есть просрочка — красный, нет — зелёный.</summary>
public sealed class OverdueDaysToBrushConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is int days && days > 0 ? Brushes.Firebrick : Brushes.ForestGreen;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => Binding.DoNothing;
}