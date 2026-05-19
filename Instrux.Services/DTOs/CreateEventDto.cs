using Instrux.Domain.Enums;

namespace Instrux.Services.DTOs;

public sealed record CreateEventDto(string Title, DateTime Date, TimeSpan? StartTime, TimeSpan? EndTime, EventCategory Category, int? LinkedClassId, string? Notes, int TeacherId);
