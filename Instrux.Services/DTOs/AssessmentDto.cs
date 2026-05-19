using Instrux.Domain.Enums;

namespace Instrux.Services.DTOs;

public sealed record AssessmentDto(int Id, int ClassId, string Name, AssessmentType Type, decimal MaxScore, decimal Weight, DateTime Date);
