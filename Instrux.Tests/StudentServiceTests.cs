using Instrux.Domain.Enums;
using Instrux.Domain.Models;
using Instrux.Infrastructure.Data;
using Instrux.Services.DTOs;
using Instrux.Services.Implementations;

namespace Instrux.Tests;

public sealed class StudentServiceTests : IDisposable
{
    private readonly InstruxDbContext _context;
    private readonly StudentService _service;
    private readonly Teacher _teacher;
    private readonly Class _class;

    public StudentServiceTests()
    {
        _context = InMemoryDbContextFactory.Create();
        _service = new StudentService(_context);
        _teacher = InMemoryDbContextFactory.CreateTeacher(_context);
        _class = InMemoryDbContextFactory.CreateClass(_context, _teacher.Id);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task Create_PersistsStudent_ReturnsDto()
    {
        var dto = new CreateStudentDto("Juan Dela Cruz", "STU-1001", "juan@test.com", _class.Id);

        var result = await _service.CreateAsync(dto);

        Assert.True(result.Id > 0);
        Assert.Equal("Juan Dela Cruz", result.FullName);
        Assert.Equal("STU-1001", result.StudentId);
        Assert.Equal("juan@test.com", result.Email);
        Assert.Equal(_class.Id, result.ClassId);
    }

    [Fact]
    public async Task GetByClass_ReturnsStudentsForClass()
    {
        await _service.CreateAsync(new CreateStudentDto("Student A", "S1", null, _class.Id));
        await _service.CreateAsync(new CreateStudentDto("Student B", "S2", null, _class.Id));

        var results = await _service.GetByClassAsync(_class.Id);

        Assert.Equal(2, results.Count);
    }

    [Fact]
    public async Task Delete_RemovesStudentAndCascadeScoresAndAttendance()
    {
        var created = await _service.CreateAsync(new CreateStudentDto("Delete Me", "S1", null, _class.Id));
        _context.AttendanceRecords.Add(new AttendanceRecord { StudentId = created.Id, Date = DateTime.Today, Status = AttendanceStatus.Present });
        var assessment = new Assessment { ClassId = _class.Id, Name = "Q", Type = AssessmentType.Quiz, MaxScore = 50, Weight = 0.40m, Date = DateTime.Today };
        _context.Assessments.Add(assessment);
        _context.SaveChanges();
        _context.Scores.Add(new Score { StudentId = created.Id, AssessmentId = assessment.Id, Value = 45 });
        _context.SaveChanges();

        await _service.DeleteAsync(created.Id);

        Assert.Empty(_context.Students.Where(s => s.Id == created.Id));
        Assert.Empty(_context.Scores.Where(s => s.StudentId == created.Id));
        Assert.Empty(_context.AttendanceRecords.Where(a => a.StudentId == created.Id));
    }

    [Fact]
    public async Task GetAll_ReturnsOnlyOwnedStudents()
    {
        var otherTeacher = InMemoryDbContextFactory.CreateTeacher(_context, "other@test.com");
        var otherClass = InMemoryDbContextFactory.CreateClass(_context, otherTeacher.Id);

        await _service.CreateAsync(new CreateStudentDto("My Student", "S1", null, _class.Id));
        await _service.CreateAsync(new CreateStudentDto("Other's Student", "S2", null, otherClass.Id));

        var results = await _service.GetAllAsync(_teacher.Id);

        Assert.Single(results);
        Assert.Equal("My Student", results[0].FullName);
    }
}
