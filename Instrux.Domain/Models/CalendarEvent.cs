using Instrux.Domain.Enums;

namespace Instrux.Domain.Models;

public class CalendarEvent
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
    public EventCategory Category { get; set; }
    public int? LinkedClassId { get; set; }
    public string? Notes { get; set; }
    public int TeacherId { get; set; }
}
