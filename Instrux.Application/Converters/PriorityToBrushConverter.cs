using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Instrux.Domain.Enums;

namespace Instrux.Application.Converters;

public sealed class PriorityToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value switch
    {
        Priority.Low => BrushFrom("#3BA776"),
        Priority.Medium => BrushFrom("#D99B2B"),
        Priority.High => BrushFrom("#E86D5D"),
        Priority.Urgent => BrushFrom("#C23357"),
        _ => BrushFrom("#6B7280")
    };

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;

    private static SolidColorBrush BrushFrom(string hex) => new((Color)ColorConverter.ConvertFromString(hex));
}
