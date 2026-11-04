using System.Globalization;

namespace Evacuation.Mobile.Converters;

/// <summary>Negacja wartości logicznej — do wiązań typu „pokaż, gdy czegoś nie ma”.</summary>
public sealed class InverseBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is not true;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is not true;
}
