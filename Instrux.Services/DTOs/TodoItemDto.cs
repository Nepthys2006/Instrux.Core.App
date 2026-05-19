using Instrux.Domain.Enums;

namespace Instrux.Services.DTOs;

public sealed record TodoItemDto(int Id, string Title, DateTime? DueDate, Priority Priority, int? LinkedClassId, bool IsCompleted, DateTime? CompletedAt, bool IsRecurring, RecurrenceType? Recurrence, int TeacherId);
