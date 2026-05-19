using Instrux.Domain.Enums;
using Instrux.Domain.Models;
using Instrux.Infrastructure.Data;
using Instrux.Services.DTOs;
using Instrux.Services.Implementations;

namespace Instrux.Tests;

public sealed class TeacherServiceTests : IDisposable
{
    private readonly InstruxDbContext _context;
    private readonly TeacherService _service;
    private readonly Teacher _teacher;

    public TeacherServiceTests()
    {
        _context = InMemoryDbContextFactory.Create();
        _service = new TeacherService(_context);
        _teacher = InMemoryDbContextFactory.CreateTeacher(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task GetProfile_ReturnsTeacher()
    {
        var result = await _service.GetProfileAsync(_teacher.Id);

        Assert.NotNull(result);
        Assert.Equal(_teacher.FullName, result.FullName);
        Assert.Equal(_teacher.Nickname, result.Nickname);
        Assert.Equal(_teacher.Email, result.Email);
    }

    [Fact]
    public async Task GetProfile_NotFound_ReturnsNull()
    {
        var result = await _service.GetProfileAsync(999);
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateProfile_UpdatesAndReturns()
    {
        var dto = new TeacherDto(_teacher.Id, "Updated Name", "Updated Nick", "updated@test.com");

        var result = await _service.UpdateProfileAsync(dto);

        Assert.Equal("Updated Name", result.FullName);
        Assert.Equal("Updated Nick", result.Nickname);
        Assert.Equal("updated@test.com", result.Email);

        var fetched = await _service.GetProfileAsync(_teacher.Id);
        Assert.Equal("Updated Name", fetched!.FullName);
    }

    [Fact]
    public async Task UpdateProfile_NotFound_Throws()
    {
        var dto = new TeacherDto(999, "Ghost", "G", "ghost@test.com");

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdateProfileAsync(dto));
    }

    [Fact]
    public async Task DeleteAccount_RemovesAllData()
    {
        var classItem = InMemoryDbContextFactory.CreateClass(_context, _teacher.Id);
        var student = InMemoryDbContextFactory.CreateStudent(_context, classItem.Id);
        var assessment = new Assessment { ClassId = classItem.Id, Name = "Quiz", Type = AssessmentType.Quiz, MaxScore = 50, Weight = 0.40m, Date = DateTime.Today };
        _context.Assessments.Add(assessment);
        _context.SaveChanges();
        _context.Scores.Add(new Score { StudentId = student.Id, AssessmentId = assessment.Id, Value = 45 });
        _context.AttendanceRecords.Add(new AttendanceRecord { StudentId = student.Id, Date = DateTime.Today, Status = AttendanceStatus.Present });
        _context.ContentItems.Add(new ContentItem { ClassId = classItem.Id, Title = "File", Description = "desc", Type = ContentType.Pdf, FilePath = "path", UploadedAt = DateTime.Now, IsVisible = true });
        _context.CalendarEvents.Add(new CalendarEvent { TeacherId = _teacher.Id, Title = "Event", Date = DateTime.Today, Category = EventCategory.Meeting });
        _context.TodoItems.Add(new TodoItem { TeacherId = _teacher.Id, Title = "Task", Priority = Priority.Medium });
        _context.SaveChanges();

        await _service.DeleteAccountAsync(_teacher.Id);

        Assert.Empty(_context.Teachers.ToList());
        Assert.Empty(_context.Classes.ToList());
        Assert.Empty(_context.Students.ToList());
        Assert.Empty(_context.Assessments.ToList());
        Assert.Empty(_context.Scores.ToList());
        Assert.Empty(_context.AttendanceRecords.ToList());
        Assert.Empty(_context.ContentItems.ToList());
        Assert.Empty(_context.CalendarEvents.ToList());
        Assert.Empty(_context.TodoItems.ToList());
    }

    [Fact]
    public async Task DeleteAccount_LeavesOtherTeachersData()
    {
        var otherTeacher = InMemoryDbContextFactory.CreateTeacher(_context, "other@test.com");
        var otherClass = new Class { Name = "Other Class", Subject = Subject.Science, TeacherId = otherTeacher.Id };
        _context.Classes.Add(otherClass);
        _context.SaveChanges();

        await _service.DeleteAccountAsync(_teacher.Id);

        Assert.Single(_context.Teachers);
        Assert.Equal(otherTeacher.Id, _context.Teachers.First().Id);
        Assert.Single(_context.Classes);
    }
}
