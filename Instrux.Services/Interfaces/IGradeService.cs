using Instrux.Services.DTOs;

namespace Instrux.Services.Interfaces;

public interface IGradeService
{
    Task<List<AssessmentDto>> GetAssessmentsAsync(int classId);
    Task<List<AssessmentDto>> GetAllAssessmentsAsync(int teacherId);
    Task<List<ScoreDto>> GetAllScoresAsync(int teacherId);
    Task<AssessmentDto> CreateAssessmentAsync(AssessmentDto assessment);
    Task DeleteAssessmentAsync(int assessmentId);
    Task<ScoreDto> UpdateScoreAsync(ScoreDto score);
    Task<List<GradeBookRowDto>> GetGradeBookAsync(int classId);
}
