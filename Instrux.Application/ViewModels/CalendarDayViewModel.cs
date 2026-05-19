using System.Collections.ObjectModel;
using Instrux.Domain.Models;

namespace Instrux.Application.ViewModels;

public sealed class CalendarDayViewModel
{
    public DateTime Date { get; init; }
    public bool IsCurrentMonth { get; init; }
    public bool IsSelected { get; init; }
    public bool IsToday => Date.Date == DateTime.Today;
    public ObservableCollection<CalendarEvent> Events { get; } = [];
}
