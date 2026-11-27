using System.Globalization;
using System.Windows.Data;

namespace Library.App.Converters;

/// <summary>Дата или «—», если значения нет (фактическая дата возврата).</summary>
public sealed class NullableDateConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is DateTime date ? date.ToString("dd.MM.yyyy", culture) : "—";

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => Binding.DoNothing;
}