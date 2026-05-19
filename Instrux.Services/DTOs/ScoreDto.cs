namespace Instrux.Services.DTOs;

public sealed record ScoreDto(int Id, int StudentId, int AssessmentId, decimal? Value);
