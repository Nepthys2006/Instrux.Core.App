using Instrux.Domain.Enums;
using Instrux.Domain.Models;
using Instrux.Infrastructure.Repositories;
using Instrux.Services.DTOs;
using Instrux.Services.Interfaces;
using Instrux.Services.Mapping;
using Instrux.Services.Resolvers;
using Microsoft.EntityFrameworkCore;

namespace Instrux.Services.Implementations;

public sealed class GradeService : IGradeService
{
    private readonly IRepository _repo;

    public GradeService(IRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<AssessmentDto>> GetAssessmentsAsync(int classId)
    {
        var items = await _repo.FindAsync<Assessment>(item => item.ClassId == classId);
        return items.OrderBy(item => item.Date).Select(DtoMapper.ToDto).ToList();
    }

    public async Task<List<AssessmentDto>> GetAllAssessmentsAsync(int teacherId) => (await _repo.Query<Assessment>()
        .Where(item => _repo.Query<Class>().Any(classItem => classItem.Id == item.ClassId && classItem.TeacherId == teacherId))
        .ToListAsync())
        .Select(DtoMapper.ToDto)
        .ToList();

    public async Task<List<ScoreDto>> GetAllScoresAsync(int teacherId) => (await _repo.Query<Score>()
        .Where(score => _repo.Query<Student>().Any(student => student.Id == score.StudentId
            && _repo.Query<Class>().Any(classItem => classItem.Id == student.ClassId && classItem.TeacherId == teacherId)))
        .ToListAsync())
        .Select(DtoMapper.ToDto)
        .ToList();

    public async Task<AssessmentDto> CreateAssessmentAsync(AssessmentDto assessment)
    {
        var entity = DtoMapper.ToEntity(assessment);
        entity.Id = 0;
        _repo.Add(entity);
        await _repo.SaveChangesAsync();
        return DtoMapper.ToDto(entity);
    }

    public async Task DeleteAssessmentAsync(int assessmentId)
    {
        var scores = await _repo.FindAsync<Score>(s => s.AssessmentId == assessmentId);
        _repo.DeleteRange(scores);
        var assessment = await _repo.GetByIdAsync<Assessment>(assessmentId);
        if (assessment is not null)
        {
            _repo.Delete(assessment);
        }
        await _repo.SaveChangesAsync();
    }

    public async Task<ScoreDto> UpdateScoreAsync(ScoreDto score)
    {
        var entity = await _repo.FirstOrDefaultAsync<Score>(item => item.StudentId == score.StudentId && item.AssessmentId == score.AssessmentId);
        if (entity is null)
        {
            entity = new Score { StudentId = score.StudentId, AssessmentId = score.AssessmentId };
            _repo.Add(entity);
        }

        entity.Value = score.Value;
        await _repo.SaveChangesAsync();
        return DtoMapper.ToDto(entity);
    }

    public async Task<List<GradeBookRowDto>> GetGradeBookAsync(int classId)
    {
        var classItem = await _repo.GetByIdAsync<Class>(classId);
        if (classItem is null)
        {
            return [];
        }

        var config = GradingSystemResolver.GetWeightsForSubject(classItem.Subject);
        var students = await _repo.FindAsync<Student>(item => item.ClassId == classId);
        var orderedStudents = students.OrderBy(item => item.FullName).ToList();
        var assessments = await _repo.FindAsync<Assessment>(item => item.ClassId == classId);
        var assessmentIds = assessments.Select(item => item.Id).ToList();
        var scores = await _repo.FindAsync<Score>(item => assessmentIds.Contains(item.AssessmentId));

        return orderedStudents.Select(student =>
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

    private static decimal AverageCategory(AssessmentType type, IReadOnlyCollection<Assessment> assessments, IReadOnlyCollection<Score> scores)
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
