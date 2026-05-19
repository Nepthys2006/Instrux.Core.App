using Instrux.Domain.Enums;

namespace Instrux.Services.DTOs;

public sealed record CalendarEventDto(int Id, string Title, DateTime Date, TimeSpan? StartTime, TimeSpan? EndTime, EventCategory Category, int? LinkedClassId, string? Notes, int TeacherId);
