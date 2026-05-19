using System.Collections.ObjectModel;
using Instrux.Domain.Enums;
using Instrux.Domain.Models;
using Instrux.Services.DTOs;
using Instrux.Services.Interfaces;

namespace Instrux.Application.Services;

public sealed class DataService
{
    private readonly SessionService _sessionService;
    private readonly IClassService _classService;
    private readonly IStudentService _studentService;
    private readonly IAttendanceService _attendanceService;
    private readonly IGradeService _gradeService;
    private readonly ICalendarEventService _calendarEventService;
    private readonly ITodoService _todoService;
    private readonly IContentService _contentService;

    public DataService(
        SessionService sessionService,
        IClassService classService,
        IStudentService studentService,
        IAttendanceService attendanceService,
        IGradeService gradeService,
        ICalendarEventService calendarEventService,
        ITodoService todoService,
        IContentService contentService)
    {
        _sessionService = sessionService;
        _classService = classService;
        _studentService = studentService;
        _attendanceService = attendanceService;
        _gradeService = gradeService;
        _calendarEventService = calendarEventService;
        _todoService = todoService;
        _contentService = contentService;
    }

    public ObservableCollection<Class> Classes { get; } = [];
    public ObservableCollection<Student> Students { get; } = [];
    public ObservableCollection<AttendanceRecord> Attendance { get; } = [];
    public ObservableCollection<Assessment> Assessments { get; } = [];
    public ObservableCollection<Score> Scores { get; } = [];
    public ObservableCollection<ContentItem> ContentItems { get; } = [];
    public ObservableCollection<CalendarEvent> Events { get; } = [];
    public ObservableCollection<TodoItem> Todos { get; } = [];

    public void Clear()
    {
        Classes.Clear();
        Students.Clear();
        Attendance.Clear();
        Assessments.Clear();
        Scores.Clear();
        ContentItems.Clear();
        Events.Clear();
        Todos.Clear();
    }

    public async Task InitializeAsync()
    {
        var teacherId = _sessionService.CurrentTeacher.Id;
        Replace(Classes, (await _classService.GetAllAsync(teacherId)).Select(ToDomain));
        Replace(Students, (await _studentService.GetAllAsync(teacherId)).Select(ToDomain));
        Replace(Attendance, (await _attendanceService.GetAllAsync(teacherId)).Select(ToDomain));
        Replace(Assessments, (await _gradeService.GetAllAssessmentsAsync(teacherId)).Select(ToDomain));
        Replace(Scores, (await _gradeService.GetAllScoresAsync(teacherId)).Select(ToDomain));
        Replace(ContentItems, (await _contentService.GetAllAsync(teacherId)).Select(ToDomain));
        Replace(Events, (await _calendarEventService.GetAllAsync(teacherId)).Select(ToDomain));
        Replace(Todos, (await _todoService.GetAllAsync(teacherId)).Select(ToDomain));
    }

    public IEnumerable<Student> GetStudentsForClass(int classId) => Students.Where(student => student.ClassId == classId);

    public IEnumerable<Assessment> GetAssessmentsForClass(int classId) => Assessments.Where(assessment => assessment.ClassId == classId);

    public IEnumerable<ContentItem> GetContentForClass(int classId) => ContentItems.Where(item => item.ClassId == classId);

    public IEnumerable<AttendanceRecord> GetAttendanceForDate(DateTime date) => Attendance.Where(record => record.Date.Date == date.Date);

    public async Task<Class> AddClassAsync(Class classItem)
    {
        var created = await _classService.CreateAsync(new CreateClassDto(classItem.Name, classItem.Section, classItem.Subject, classItem.SchoolYear, classItem.Semester, classItem.CoverColor, _sessionService.CurrentTeacher.Id));
        var domain = ToDomain(created);
        Classes.Add(domain);
        return domain;
    }

    public async Task DeleteClassAsync(Class classItem)
    {
        await _classService.DeleteAsync(classItem.Id);
        Classes.Remove(classItem);

        var removedStudentIds = Students.Where(item => item.ClassId == classItem.Id).Select(item => item.Id).ToHashSet();
        var removedAssessmentIds = Assessments.Where(item => item.ClassId == classItem.Id).Select(item => item.Id).ToHashSet();

        Replace(Students, Students.Where(item => item.ClassId != classItem.Id).ToList());
        Replace(Assessments, Assessments.Where(item => item.ClassId != classItem.Id).ToList());
        Replace(ContentItems, ContentItems.Where(item => item.ClassId != classItem.Id).ToList());
        Replace(Attendance, Attendance.Where(item => !removedStudentIds.Contains(item.StudentId)).ToList());
        Replace(Scores, Scores.Where(item => !removedStudentIds.Contains(item.StudentId) && !removedAssessmentIds.Contains(item.AssessmentId)).ToList());

        foreach (var calendarEvent in Events.Where(item => item.LinkedClassId == classItem.Id))
        {
            calendarEvent.LinkedClassId = null;
        }

        foreach (var todo in Todos.Where(item => item.LinkedClassId == classItem.Id))
        {
            todo.LinkedClassId = null;
        }
    }

    public async Task<Student> AddStudentAsync(Student student)
    {
        var created = await _studentService.CreateAsync(new CreateStudentDto(student.FullName, student.StudentId, student.Email, student.ClassId));
        var domain = ToDomain(created);
        Students.Add(domain);
        return domain;
    }

    public async Task DeleteStudentAsync(Student student)
    {
        await _studentService.DeleteAsync(student.Id);
        Students.Remove(student);

        Replace(Attendance, Attendance.Where(item => item.StudentId != student.Id).ToList());
        Replace(Scores, Scores.Where(item => item.StudentId != student.Id).ToList());
    }

    public async Task AddTodoAsync(TodoItem todo)
    {
        var created = await _todoService.CreateAsync(new CreateTodoDto(todo.Title, todo.DueDate, todo.Priority, todo.LinkedClassId, _sessionService.CurrentTeacher.Id));
        Todos.Insert(0, ToDomain(created));
    }

    public async Task ToggleTodoAsync(TodoItem todo)
    {
        var saved = await _todoService.ToggleAsync(todo.Id);
        todo.IsCompleted = saved.IsCompleted;
        todo.CompletedAt = saved.CompletedAt;
    }

    public async Task DeleteTodoAsync(TodoItem todo)
    {
        await _todoService.DeleteAsync(todo.Id);
        Todos.Remove(todo);
    }

    public async Task<CalendarEvent> AddEventAsync(CalendarEvent calendarEvent)
    {
        var created = await _calendarEventService.CreateAsync(new CreateEventDto(calendarEvent.Title, calendarEvent.Date, calendarEvent.StartTime, calendarEvent.EndTime, calendarEvent.Category, calendarEvent.LinkedClassId, calendarEvent.Notes, _sessionService.CurrentTeacher.Id));
        var domain = ToDomain(created);
        Events.Add(domain);
        return domain;
    }

    public async Task DeleteEventAsync(CalendarEvent calendarEvent)
    {
        await _calendarEventService.DeleteAsync(calendarEvent.Id);
        Events.Remove(calendarEvent);
    }

    public async Task<Assessment> AddAssessmentAsync(Assessment assessment)
    {
        var created = await _gradeService.CreateAssessmentAsync(new AssessmentDto(0, assessment.ClassId, assessment.Name, assessment.Type, assessment.MaxScore, assessment.Weight, assessment.Date));
        var domain = ToDomain(created);
        Assessments.Add(domain);
        return domain;
    }

    public async Task DeleteAssessmentAsync(Assessment assessment)
    {
        await _gradeService.DeleteAssessmentAsync(assessment.Id);
        Assessments.Remove(assessment);
        Replace(Scores, Scores.Where(s => s.AssessmentId != assessment.Id).ToList());
    }

    public async Task SaveScoreAsync(int studentId, int assessmentId, decimal? value)
    {
        var saved = await _gradeService.UpdateScoreAsync(new ScoreDto(0, studentId, assessmentId, value));
        var local = Scores.FirstOrDefault(item => item.StudentId == studentId && item.AssessmentId == assessmentId);
        if (local is null)
        {
            Scores.Add(ToDomain(saved));
        }
        else
        {
            local.Value = value;
        }
    }

    public async Task<ContentItem> AddContentItemAsync(ContentItem content)
    {
        var created = await _contentService.CreateAsync(new CreateContentItemDto(content.ClassId, content.FolderId, content.Title, content.Description, content.Type, content.FilePath, content.IsVisible));
        var domain = ToDomain(created);
        ContentItems.Insert(0, domain);
        return domain;
    }

    public async Task DeleteContentItemAsync(ContentItem content)
    {
        await _contentService.DeleteAsync(content.Id);
        ContentItems.Remove(content);
    }

    public async Task SaveAttendanceRecordAsync(int studentId, DateTime date, AttendanceStatus status)
    {
        var saved = await _attendanceService.SaveRecordAsync(studentId, date.Date, status);
        var local = Attendance.FirstOrDefault(item => item.StudentId == studentId && item.Date.Date == date.Date);
        if (local is null)
        {
            Attendance.Add(ToDomain(saved));
        }
        else
        {
            local.Status = status;
        }
    }

    private static void Replace<T>(ObservableCollection<T> target, IEnumerable<T> values)
    {
        target.Clear();
        foreach (var value in values)
        {
            target.Add(value);
        }
    }

    private static Class ToDomain(ClassDto dto) => new()
    {
        Id = dto.Id,
        Name = dto.Name,
        Section = dto.Section,
        Subject = dto.Subject,
        SchoolYear = dto.SchoolYear,
        Semester = dto.Semester,
        CoverColor = dto.CoverColor,
        TeacherId = dto.TeacherId
    };

    private static Student ToDomain(StudentDto dto) => new() { Id = dto.Id, FullName = dto.FullName, StudentId = dto.StudentId, Email = dto.Email, ClassId = dto.ClassId };

    private static AttendanceRecord ToDomain(AttendanceRecordDto dto) => new() { Id = dto.Id, StudentId = dto.StudentId, Date = dto.Date, Status = dto.Status, Note = dto.Note };

    private static Assessment ToDomain(AssessmentDto dto) => new() { Id = dto.Id, ClassId = dto.ClassId, Name = dto.Name, Type = dto.Type, MaxScore = dto.MaxScore, Weight = dto.Weight, Date = dto.Date };

    private static Score ToDomain(ScoreDto dto) => new() { Id = dto.Id, StudentId = dto.StudentId, AssessmentId = dto.AssessmentId, Value = dto.Value };

    private static ContentItem ToDomain(ContentItemDto dto) => new() { Id = dto.Id, ClassId = dto.ClassId, FolderId = dto.FolderId, Title = dto.Title, Description = dto.Description, Type = dto.Type, FilePath = dto.FilePath, UploadedAt = dto.UploadedAt, IsVisible = dto.IsVisible };

    private static CalendarEvent ToDomain(CalendarEventDto dto) => new() { Id = dto.Id, Title = dto.Title, Date = dto.Date, StartTime = dto.StartTime, EndTime = dto.EndTime, Category = dto.Category, LinkedClassId = dto.LinkedClassId, Notes = dto.Notes, TeacherId = dto.TeacherId };

    private static TodoItem ToDomain(TodoItemDto dto) => new() { Id = dto.Id, Title = dto.Title, DueDate = dto.DueDate, Priority = dto.Priority, LinkedClassId = dto.LinkedClassId, IsCompleted = dto.IsCompleted, CompletedAt = dto.CompletedAt, IsRecurring = dto.IsRecurring, Recurrence = dto.Recurrence, TeacherId = dto.TeacherId };
}
