namespace Instrux.Services.DTOs;

public sealed record GradeBookRowDto(int StudentId, string StudentName, Dictionary<int, decimal?> Scores, decimal WrittenWorksAverage, decimal PerformanceTasksAverage, decimal QuarterlyAssessmentAverage, decimal InitialGrade, string Standing);
