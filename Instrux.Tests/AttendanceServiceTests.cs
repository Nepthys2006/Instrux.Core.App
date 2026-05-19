using Instrux.Domain.Enums;
using Instrux.Domain.Models;
using Instrux.Infrastructure.Data;
using Instrux.Infrastructure.Repositories;
using Instrux.Services.Implementations;

namespace Instrux.Tests;

public sealed class AttendanceServiceTests : IDisposable
{
    private readonly InstruxDbContext _context;
    private readonly AttendanceService _service;
    private readonly Teacher _teacher;
    private readonly Class _class;
    private readonly Student _student;

    public AttendanceServiceTests()
    {
        _context = InMemoryDbContextFactory.Create();
        _service = new AttendanceService(new Repository(_context));
        _teacher = InMemoryDbContextFactory.CreateTeacher(_context);
        _class = InMemoryDbContextFactory.CreateClass(_context, _teacher.Id);
        _student = InMemoryDbContextFactory.CreateStudent(_context, _class.Id);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task SaveRecord_CreatesNew_WhenNoExisting()
    {
        var result = await _service.SaveRecordAsync(_student.Id, DateTime.Today, AttendanceStatus.Present);

        Assert.Equal(_student.Id, result.StudentId);
        Assert.Equal(DateTime.Today, result.Date.Date);
        Assert.Equal(AttendanceStatus.Present, result.Status);
    }

    [Fact]
    public async Task SaveRecord_Updates_WhenExisting()
    {
        await _service.SaveRecordAsync(_student.Id, DateTime.Today, AttendanceStatus.Present);

        var result = await _service.SaveRecordAsync(_student.Id, DateTime.Today, AttendanceStatus.Late);

        Assert.Equal(AttendanceStatus.Late, result.Status);
    }

    [Fact]
    public async Task GetByDate_ReturnsRecordsForClassAndDate()
    {
        var otherStudent = new Instrux.Domain.Models.Student { FullName = "Other", StudentId = "S2", ClassId = _class.Id };
        _context.Students.Add(otherStudent);
        _context.SaveChanges();

        await _service.SaveRecordAsync(_student.Id, DateTime.Today, AttendanceStatus.Present);
        await _service.SaveRecordAsync(otherStudent.Id, DateTime.Today, AttendanceStatus.Absent);

        var results = await _service.GetByDateAsync(_class.Id, DateTime.Today);

        Assert.Equal(2, results.Count);
    }

    [Fact]
    public async Task GetByDate_FiltersByDate()
    {
        await _service.SaveRecordAsync(_student.Id, DateTime.Today, AttendanceStatus.Present);
        await _service.SaveRecordAsync(_student.Id, DateTime.Today.AddDays(-1), AttendanceStatus.Late);

        var results = await _service.GetByDateAsync(_class.Id, DateTime.Today);

        Assert.Single(results);
        Assert.Equal(AttendanceStatus.Present, results[0].Status);
    }

    [Fact]
    public async Task GetAll_ReturnsOnlyOwnedRecords()
    {
        var otherTeacher = InMemoryDbContextFactory.CreateTeacher(_context, "other@test.com");
        var otherClass = InMemoryDbContextFactory.CreateClass(_context, otherTeacher.Id);
        var otherStudent = InMemoryDbContextFactory.CreateStudent(_context, otherClass.Id, "Other Student");

        await _service.SaveRecordAsync(_student.Id, DateTime.Today, AttendanceStatus.Present);
        await _service.SaveRecordAsync(otherStudent.Id, DateTime.Today, AttendanceStatus.Absent);

        var results = await _service.GetAllAsync(_teacher.Id);

        Assert.Single(results);
        Assert.Equal(AttendanceStatus.Present, results[0].Status);
    }

    [Fact]
    public async Task SaveRecord_IncludesNote()
    {
        var result = await _service.SaveRecordAsync(_student.Id, DateTime.Today, AttendanceStatus.Excused, "Medical appointment");

        Assert.Equal("Medical appointment", result.Note);
    }
}
