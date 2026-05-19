using Instrux.Domain.Enums;
using Instrux.Domain.Models;
using Instrux.Infrastructure.Data;
using Instrux.Services.DTOs;
using Instrux.Services.Implementations;

namespace Instrux.Tests;

public sealed class ClassServiceTests : IDisposable
{
    private readonly InstruxDbContext _context;
    private readonly ClassService _service;
    private readonly Teacher _teacher;

    public ClassServiceTests()
    {
        _context = InMemoryDbContextFactory.Create();
        _service = new ClassService(_context);
        _teacher = InMemoryDbContextFactory.CreateTeacher(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task Create_PersistsClass_ReturnsDto()
    {
        var dto = new CreateClassDto("Grade 8 Math", "Section A", Subject.Mathematics, "2025-2026", "1st", "#2C5EAD", _teacher.Id);

        var result = await _service.CreateAsync(dto);

        Assert.True(result.Id > 0);
        Assert.Equal("Grade 8 Math", result.Name);
        Assert.Equal("Section A", result.Section);
        Assert.Equal(Subject.Mathematics, result.Subject);
        Assert.Equal(_teacher.Id, result.TeacherId);
    }

    [Fact]
    public async Task GetAll_ReturnsOnlyTeachersClasses()
    {
        var otherTeacher = InMemoryDbContextFactory.CreateTeacher(_context, "other@test.com");

        await _service.CreateAsync(new CreateClassDto("My Class", "A", Subject.Mathematics, "2025", "1st", "#000", _teacher.Id));
        await _service.CreateAsync(new CreateClassDto("Other's Class", "B", Subject.Science, "2025", "1st", "#000", otherTeacher.Id));

        var results = await _service.GetAllAsync(_teacher.Id);

        Assert.Single(results);
        Assert.Equal("My Class", results[0].Name);
    }

    [Fact]
    public async Task GetById_ReturnsCorrectClass()
    {
        var created = await _service.CreateAsync(new CreateClassDto("Test Class", "A", Subject.MAPEH, "2025", "1st", "#fff", _teacher.Id));

        var result = await _service.GetByIdAsync(created.Id);

        Assert.NotNull(result);
        Assert.Equal("Test Class", result.Name);
    }

    [Fact]
    public async Task GetById_NotFound_ReturnsNull()
    {
        var result = await _service.GetByIdAsync(999);
        Assert.Null(result);
    }

    [Fact]
    public async Task Delete_RemovesClassAndCascadeData()
    {
        var created = await _service.CreateAsync(new CreateClassDto("Delete Me", "A", Subject.Mathematics, "2025", "1st", "#000", _teacher.Id));
        var student = new Student { FullName = "Test", StudentId = "S1", ClassId = created.Id };
        _context.Students.Add(student);
        _context.SaveChanges();
        var assessment = new Assessment { ClassId = created.Id, Name = "Q", Type = AssessmentType.Quiz, MaxScore = 50, Weight = 0.40m, Date = DateTime.Today };
        _context.Assessments.Add(assessment);
        _context.SaveChanges();
        _context.Scores.Add(new Score { StudentId = student.Id, AssessmentId = assessment.Id, Value = 40 });
        _context.AttendanceRecords.Add(new AttendanceRecord { StudentId = student.Id, Date = DateTime.Today, Status = AttendanceStatus.Present });
        _context.ContentItems.Add(new ContentItem { ClassId = created.Id, Title = "F", Description = "d", Type = ContentType.Pdf, FilePath = "p", UploadedAt = DateTime.Now, IsVisible = true });
        _context.SaveChanges();

        await _service.DeleteAsync(created.Id);

        Assert.Empty(_context.Classes.Where(c => c.Id == created.Id));
        Assert.Empty(_context.Students.Where(s => s.ClassId == created.Id));
        Assert.Empty(_context.Assessments.Where(a => a.ClassId == created.Id));
        Assert.Empty(_context.Scores.Where(s => s.AssessmentId == assessment.Id));
        Assert.Empty(_context.AttendanceRecords.Where(a => a.StudentId == student.Id));
        Assert.Empty(_context.ContentItems.Where(c => c.ClassId == created.Id));
    }
}
