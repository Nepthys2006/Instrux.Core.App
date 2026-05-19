using Instrux.Domain.Enums;
using Instrux.Domain.Models;
using Instrux.Infrastructure.Data;
using Instrux.Services.DTOs;
using Instrux.Services.Implementations;

namespace Instrux.Tests;

public sealed class GradeServiceTests : IDisposable
{
    private readonly InstruxDbContext _context;
    private readonly GradeService _service;
    private readonly Teacher _teacher;
    private readonly Class _class;
    private readonly Student _student;

    public GradeServiceTests()
    {
        _context = InMemoryDbContextFactory.Create();
        _service = new GradeService(_context);
        _teacher = InMemoryDbContextFactory.CreateTeacher(_context);
        _class = InMemoryDbContextFactory.CreateClass(_context, _teacher.Id, Subject.Mathematics);
        _student = InMemoryDbContextFactory.CreateStudent(_context, _class.Id);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task CreateAssessment_PersistsAndReturnsDto()
    {
        var dto = new AssessmentDto(0, _class.Id, "Quiz 1", AssessmentType.Quiz, 50m, 0.40m, DateTime.Today);

        var result = await _service.CreateAssessmentAsync(dto);

        Assert.True(result.Id > 0);
        Assert.Equal(_class.Id, result.ClassId);
        Assert.Equal("Quiz 1", result.Name);
        Assert.Equal(AssessmentType.Quiz, result.Type);
        Assert.Equal(50m, result.MaxScore);
    }

    [Fact]
    public async Task GetAssessments_ReturnsAssessmentsForClass()
    {
        await _service.CreateAssessmentAsync(new AssessmentDto(0, _class.Id, "Quiz 1", AssessmentType.Quiz, 50m, 0.40m, DateTime.Today));
        await _service.CreateAssessmentAsync(new AssessmentDto(0, _class.Id, "Activity 1", AssessmentType.Activity, 30m, 0.60m, DateTime.Today));

        var results = await _service.GetAssessmentsAsync(_class.Id);

        Assert.Equal(2, results.Count);
    }

    [Fact]
    public async Task DeleteAssessment_RemovesAssessmentAndScores()
    {
        var assessment = await _service.CreateAssessmentAsync(new AssessmentDto(0, _class.Id, "Quiz 1", AssessmentType.Quiz, 50m, 0.40m, DateTime.Today));
        await _service.UpdateScoreAsync(new ScoreDto(0, _student.Id, assessment.Id, 45));

        await _service.DeleteAssessmentAsync(assessment.Id);

        var assessments = await _service.GetAssessmentsAsync(_class.Id);
        Assert.Empty(assessments);

        var allScores = await _service.GetAllScoresAsync(_teacher.Id);
        Assert.DoesNotContain(allScores, s => s.AssessmentId == assessment.Id);
    }

    [Fact]
    public async Task GetGradeBook_Computes_81Percent_OnTrack()
    {
        var quiz = await _service.CreateAssessmentAsync(new AssessmentDto(0, _class.Id, "Quiz 1", AssessmentType.Quiz, 50m, 0.40m, DateTime.Today));
        var activity = await _service.CreateAssessmentAsync(new AssessmentDto(0, _class.Id, "Activity 1", AssessmentType.Activity, 50m, 0.40m, DateTime.Today));
        var exam = await _service.CreateAssessmentAsync(new AssessmentDto(0, _class.Id, "Exam 1", AssessmentType.Exam, 50m, 0.20m, DateTime.Today));

        await _service.UpdateScoreAsync(new ScoreDto(0, _student.Id, quiz.Id, 45));
        await _service.UpdateScoreAsync(new ScoreDto(0, _student.Id, activity.Id, 40));
        await _service.UpdateScoreAsync(new ScoreDto(0, _student.Id, exam.Id, 35));

        var gradeBook = await _service.GetGradeBookAsync(_class.Id);

        var row = Assert.Single(gradeBook);
        Assert.Equal(_student.Id, row.StudentId);
        Assert.Equal(_student.FullName, row.StudentName);

        Assert.Equal(90m, row.WrittenWorksAverage);
        Assert.Equal(80m, row.PerformanceTasksAverage);
        Assert.Equal(70m, row.QuarterlyAssessmentAverage);

        var expected = (90m * 0.40m) + (80m * 0.40m) + (70m * 0.20m);
        Assert.Equal(82m, expected);
        Assert.Equal(82m, row.InitialGrade);
        Assert.Equal("On track", row.Standing);
    }

    [Fact]
    public async Task GetGradeBook_NoScores_ReturnsZero()
    {
        await _service.CreateAssessmentAsync(new AssessmentDto(0, _class.Id, "Quiz 1", AssessmentType.Quiz, 50m, 0.40m, DateTime.Today));

        var gradeBook = await _service.GetGradeBookAsync(_class.Id);

        var row = Assert.Single(gradeBook);
        Assert.Equal(0, row.WrittenWorksAverage);
        Assert.Equal(0, row.PerformanceTasksAverage);
        Assert.Equal(0, row.QuarterlyAssessmentAverage);
        Assert.Equal(0, row.InitialGrade);
        Assert.Equal("Support", row.Standing);
    }

    [Fact]
    public async Task GetGradeBook_UnknownClass_ReturnsEmpty()
    {
        var result = await _service.GetGradeBookAsync(999);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetGradeBook_StandingThresholds()
    {
        var quiz = await _service.CreateAssessmentAsync(new AssessmentDto(0, _class.Id, "Quiz", AssessmentType.Quiz, 50m, 0.40m, DateTime.Today));
        var activity = await _service.CreateAssessmentAsync(new AssessmentDto(0, _class.Id, "Act", AssessmentType.Activity, 50m, 0.40m, DateTime.Today));
        var exam = await _service.CreateAssessmentAsync(new AssessmentDto(0, _class.Id, "Exam", AssessmentType.Exam, 50m, 0.20m, DateTime.Today));

        // Test: all perfect scores → 100% → Excellent
        await _service.UpdateScoreAsync(new ScoreDto(0, _student.Id, quiz.Id, 50));
        await _service.UpdateScoreAsync(new ScoreDto(0, _student.Id, activity.Id, 50));
        await _service.UpdateScoreAsync(new ScoreDto(0, _student.Id, exam.Id, 50));

        var gradeBook = await _service.GetGradeBookAsync(_class.Id);
        Assert.Equal("Excellent", gradeBook[0].Standing);
    }

    [Fact]
    public async Task UpdateScore_CreatesNew_WhenNoExisting()
    {
        var assessment = await _service.CreateAssessmentAsync(new AssessmentDto(0, _class.Id, "Quiz 1", AssessmentType.Quiz, 50m, 0.40m, DateTime.Today));

        var result = await _service.UpdateScoreAsync(new ScoreDto(0, _student.Id, assessment.Id, 45));

        Assert.Equal(_student.Id, result.StudentId);
        Assert.Equal(assessment.Id, result.AssessmentId);
        Assert.Equal(45, result.Value);
    }

    [Fact]
    public async Task UpdateScore_Updates_WhenExisting()
    {
        var assessment = await _service.CreateAssessmentAsync(new AssessmentDto(0, _class.Id, "Quiz 1", AssessmentType.Quiz, 50m, 0.40m, DateTime.Today));
        await _service.UpdateScoreAsync(new ScoreDto(0, _student.Id, assessment.Id, 45));

        var result = await _service.UpdateScoreAsync(new ScoreDto(0, _student.Id, assessment.Id, 48));

        Assert.Equal(48, result.Value);
    }

    [Fact]
    public async Task GetAllAssessmentsAsync_ReturnsOnlyOwnedAssessments()
    {
        var otherTeacher = InMemoryDbContextFactory.CreateTeacher(_context, "other@test.com");
        var otherClass = new Class { Name = "Other", Subject = Subject.Science, TeacherId = otherTeacher.Id };
        _context.Classes.Add(otherClass);
        _context.SaveChanges();
        _context.Assessments.Add(new Assessment { ClassId = otherClass.Id, Name = "Other Quiz", Type = AssessmentType.Quiz, MaxScore = 50, Weight = 0.40m, Date = DateTime.Today });
        _context.SaveChanges();

        await _service.CreateAssessmentAsync(new AssessmentDto(0, _class.Id, "My Quiz", AssessmentType.Quiz, 50m, 0.40m, DateTime.Today));

        var results = await _service.GetAllAssessmentsAsync(_teacher.Id);

        Assert.Single(results);
        Assert.Equal("My Quiz", results[0].Name);
    }
}
