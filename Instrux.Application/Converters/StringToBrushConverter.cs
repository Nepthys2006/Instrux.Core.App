using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Instrux.Application.Converters;

public sealed class StringToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string color && !string.IsNullOrWhiteSpace(color))
        {
            try
            {
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
            }
            catch (FormatException)
            {
            }
        }

        return new SolidColorBrush(Color.FromRgb(91, 108, 255));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
}
