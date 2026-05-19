using Instrux.Domain.Models;
using Instrux.Services.DTOs;

namespace Instrux.Services.Mapping;

public static class DtoMapper
{
    public static TeacherDto ToDto(Teacher teacher) => new(teacher.Id, teacher.FullName, teacher.Nickname, teacher.Email);

    public static ClassDto ToDto(Class classItem, int studentCount = 0) => new(
        classItem.Id,
        classItem.Name,
        classItem.Section,
        classItem.Subject,
        classItem.SchoolYear,
        classItem.Semester,
        classItem.CoverColor,
        classItem.TeacherId,
        studentCount);

    public static Class ToEntity(CreateClassDto dto) => new()
    {
        Name = dto.Name,
        Section = dto.Section,
        Subject = dto.Subject,
        SchoolYear = dto.SchoolYear,
        Semester = dto.Semester,
        CoverColor = dto.CoverColor,
        TeacherId = dto.TeacherId
    };

    public static StudentDto ToDto(Student student) => new(student.Id, student.FullName, student.StudentId, student.Email, student.ClassId);

    public static Student ToEntity(CreateStudentDto dto) => new()
    {
        FullName = dto.FullName,
        StudentId = dto.StudentId,
        Email = dto.Email,
        ClassId = dto.ClassId
    };

    public static AttendanceRecordDto ToDto(AttendanceRecord record, string? studentName = null) => new(record.Id, record.StudentId, studentName, record.Date, record.Status, record.Note);

    public static AssessmentDto ToDto(Assessment assessment) => new(assessment.Id, assessment.ClassId, assessment.Name, assessment.Type, assessment.MaxScore, assessment.Weight, assessment.Date);

    public static Assessment ToEntity(AssessmentDto dto) => new()
    {
        Id = dto.Id,
        ClassId = dto.ClassId,
        Name = dto.Name,
        Type = dto.Type,
        MaxScore = dto.MaxScore,
        Weight = dto.Weight,
        Date = dto.Date
    };

    public static ScoreDto ToDto(Score score) => new(score.Id, score.StudentId, score.AssessmentId, score.Value);

    public static CalendarEventDto ToDto(CalendarEvent calendarEvent) => new(
        calendarEvent.Id,
        calendarEvent.Title,
        calendarEvent.Date,
        calendarEvent.StartTime,
        calendarEvent.EndTime,
        calendarEvent.Category,
        calendarEvent.LinkedClassId,
        calendarEvent.Notes,
        calendarEvent.TeacherId);

    public static CalendarEvent ToEntity(CreateEventDto dto) => new()
    {
        Title = dto.Title,
        Date = dto.Date,
        StartTime = dto.StartTime,
        EndTime = dto.EndTime,
        Category = dto.Category,
        LinkedClassId = dto.LinkedClassId,
        Notes = dto.Notes,
        TeacherId = dto.TeacherId
    };

    public static TodoItemDto ToDto(TodoItem todo) => new(todo.Id, todo.Title, todo.DueDate, todo.Priority, todo.LinkedClassId, todo.IsCompleted, todo.CompletedAt, todo.IsRecurring, todo.Recurrence, todo.TeacherId);

    public static TodoItem ToEntity(CreateTodoDto dto) => new()
    {
        Title = dto.Title,
        DueDate = dto.DueDate,
        Priority = dto.Priority,
        LinkedClassId = dto.LinkedClassId,
        TeacherId = dto.TeacherId
    };

    public static ContentItemDto ToDto(ContentItem content) => new(content.Id, content.ClassId, content.FolderId, content.Title, content.Description, content.Type, content.FilePath, content.UploadedAt, content.IsVisible);

    public static ContentItem ToEntity(CreateContentItemDto dto) => new()
    {
        ClassId = dto.ClassId,
        FolderId = dto.FolderId,
        Title = dto.Title,
        Description = dto.Description,
        Type = dto.Type,
        FilePath = dto.FilePath,
        UploadedAt = DateTime.Now,
        IsVisible = dto.IsVisible
    };
}
