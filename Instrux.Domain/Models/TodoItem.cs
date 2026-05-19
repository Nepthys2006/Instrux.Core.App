using Instrux.Domain.Enums;

namespace Instrux.Domain.Models;

public class TodoItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public Priority Priority { get; set; }
    public int? LinkedClassId { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
    public bool IsRecurring { get; set; }
    public RecurrenceType? Recurrence { get; set; }
    public int TeacherId { get; set; }
}
