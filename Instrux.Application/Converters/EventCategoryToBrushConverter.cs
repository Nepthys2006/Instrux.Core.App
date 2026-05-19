using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Instrux.Domain.Enums;

namespace Instrux.Application.Converters;

public sealed class EventCategoryToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value switch
    {
        EventCategory.Meeting => BrushFrom("#0C7779"),
        EventCategory.ExamDay => BrushFrom("#005461"),
        EventCategory.Holiday => BrushFrom("#3BC1A8"),
        EventCategory.Reminder => BrushFrom("#249E94"),
        EventCategory.SubmissionDeadline => BrushFrom("#D32F2F"),
        _ => BrushFrom("#5F6368")
    };

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;

    private static SolidColorBrush BrushFrom(string hex) => new((Color)ColorConverter.ConvertFromString(hex));
}
