using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Instrux.Domain.Enums;

namespace Instrux.Application.Converters;

public sealed class AttendanceStatusToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value switch
    {
        AttendanceStatus.Present => BrushFrom("#DFF7EA"),
        AttendanceStatus.Late => BrushFrom("#FFF1CF"),
        AttendanceStatus.Absent => BrushFrom("#FFE1E4"),
        AttendanceStatus.Excused => BrushFrom("#E6E9FF"),
        _ => BrushFrom("#EEF2F7")
    };

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;

    private static SolidColorBrush BrushFrom(string hex) => new((Color)ColorConverter.ConvertFromString(hex));
}
