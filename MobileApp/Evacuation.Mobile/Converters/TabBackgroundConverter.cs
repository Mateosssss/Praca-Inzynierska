using System.Globalization;

namespace Evacuation.Mobile.Converters;

/// <summary>Tło przycisku zakładki: wypełnione dla aktywnej, przezroczyste dla pozostałych.</summary>
public sealed class TabBackgroundConverter : IValueConverter
{
    private static readonly Color Active = Color.FromArgb("#7D5411");

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is true ? Active : Colors.Transparent;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
