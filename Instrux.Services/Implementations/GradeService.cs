using Instrux.Domain.Enums;
using Instrux.Infrastructure.Data;
using Instrux.Services.DTOs;
using Instrux.Services.Interfaces;
using Instrux.Services.Mapping;
using Instrux.Services.Resolvers;
using Microsoft.EntityFrameworkCore;

namespace Instrux.Services.Implementations;

public sealed class GradeService : IGradeService
{
    private readonly InstruxDbContext _dbContext;

    public GradeService(InstruxDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<AssessmentDto>> GetAssessmentsAsync(int classId) => (await _dbContext.Assessments.Where(item => item.ClassId == classId).OrderBy(item => item.Date).ToListAsync()).Select(DtoMapper.ToDto).ToList();

    public async Task<List<AssessmentDto>> GetAllAssessmentsAsync(int teacherId) => (await _dbContext.Assessments
        .Where(item => _dbContext.Classes.Any(classItem => classItem.Id == item.ClassId && classItem.TeacherId == teacherId))
        .ToListAsync())
        .Select(DtoMapper.ToDto)
        .ToList();

    public async Task<List<ScoreDto>> GetAllScoresAsync(int teacherId) => (await _dbContext.Scores
        .Where(score => _dbContext.Students.Any(student => student.Id == score.StudentId
            && _dbContext.Classes.Any(classItem => classItem.Id == student.ClassId && classItem.TeacherId == teacherId)))
        .ToListAsync())
        .Select(DtoMapper.ToDto)
        .ToList();

    public async Task<AssessmentDto> CreateAssessmentAsync(AssessmentDto assessment)
    {
        var entity = DtoMapper.ToEntity(assessment);
        entity.Id = 0;
        _dbContext.Assessments.Add(entity);
        await _dbContext.SaveChangesAsync();
        return DtoMapper.ToDto(entity);
    }

    public async Task DeleteAssessmentAsync(int assessmentId)
    {
        var scores = await _dbContext.Scores.Where(s => s.AssessmentId == assessmentId).ToListAsync();
        _dbContext.Scores.RemoveRange(scores);
        var assessment = await _dbContext.Assessments.FindAsync(assessmentId);
        if (assessment is not null)
        {
            _dbContext.Assessments.Remove(assessment);
        }
        await _dbContext.SaveChangesAsync();
    }

    public async Task<ScoreDto> UpdateScoreAsync(ScoreDto score)
    {
        var entity = await _dbContext.Scores.FirstOrDefaultAsync(item => item.StudentId == score.StudentId && item.AssessmentId == score.AssessmentId);
        if (entity is null)
        {
            entity = new Domain.Models.Score { StudentId = score.StudentId, AssessmentId = score.AssessmentId };
            _dbContext.Scores.Add(entity);
        }

        entity.Value = score.Value;
        await _dbContext.SaveChangesAsync();
        return DtoMapper.ToDto(entity);
    }

    public async Task<List<GradeBookRowDto>> GetGradeBookAsync(int classId)
    {
        var classItem = await _dbContext.Classes.FindAsync(classId);
        if (classItem is null)
        {
            return [];
        }

        var config = GradingSystemResolver.GetWeightsForSubject(classItem.Subject);
        var students = await _dbContext.Students.Where(item => item.ClassId == classId).OrderBy(item => item.FullName).ToListAsync();
        var assessments = await _dbContext.Assessments.Where(item => item.ClassId == classId).ToListAsync();
        var assessmentIds = assessments.Select(item => item.Id).ToList();
        var scores = await _dbContext.Scores.Where(item => assessmentIds.Contains(item.AssessmentId)).ToListAsync();

        return students.Select(student =>
        {
            var studentScores = scores.Where(score => score.StudentId == student.Id).ToList();
            var ww = AverageCategory(AssessmentType.Quiz, assessments, studentScores);
            var pt = AverageCategory(AssessmentType.Activity, assessments, studentScores);
            var qa = AverageCategory(AssessmentType.Exam, assessments, studentScores);
            var initialGrade = Math.Round((ww * config.WrittenWorksWeight) + (pt * config.PerformanceTasksWeight) + (qa * config.QuarterlyAssessmentWeight), 2);
            var standing = initialGrade >= 90 ? "Excellent" : initialGrade >= 80 ? "On track" : initialGrade >= 70 ? "Watch" : "Support";
            return new GradeBookRowDto(student.Id, student.FullName, studentScores.ToDictionary(score => score.AssessmentId, score => score.Value), ww, pt, qa, initialGrade, standing);
        }).ToList();
    }

    private static decimal AverageCategory(AssessmentType type, IReadOnlyCollection<Domain.Models.Assessment> assessments, IReadOnlyCollection<Domain.Models.Score> scores)
    {
        var percentages = assessments
            .Where(assessment => assessment.Type == type)
            .Select(assessment =>
            {
                var value = scores.FirstOrDefault(score => score.AssessmentId == assessment.Id)?.Value;
                return value.HasValue && assessment.MaxScore > 0 ? value.Value / assessment.MaxScore * 100 : (decimal?)null;
            })
            .Where(value => value.HasValue)
            .Select(value => value!.Value)
            .ToList();

        return percentages.Count == 0 ? 0 : Math.Round(percentages.Average(), 2);
    }
}
