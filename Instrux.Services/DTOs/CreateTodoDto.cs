using Instrux.Domain.Enums;

namespace Instrux.Services.DTOs;

public sealed record CreateTodoDto(string Title, DateTime? DueDate, Priority Priority, int? LinkedClassId, int TeacherId);
