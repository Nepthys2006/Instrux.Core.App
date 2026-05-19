using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Instrux.Application.Helpers;
using Instrux.Application.Services;
using Instrux.Domain.Enums;
using Instrux.Domain.Models;
using Instrux.Services.Exceptions;
using Microsoft.Win32;

namespace Instrux.Application.ViewModels;

public sealed class ClassesViewModel : ViewModelBase
{
    private readonly DataService _dataService;
    private readonly NotificationService _notifications;
    private Class? _selectedClass;
    private string _newClassName = string.Empty;
    private string _newClassSection = string.Empty;
    private Subject _selectedSubject = Subject.Mathematics;
    private string _classSearch = string.Empty;
    private string _newStudentName = string.Empty;
    private DateTime _attendanceDate = DateTime.Today;
    private string _newAssessmentName = string.Empty;
    private AssessmentType _newAssessmentType = AssessmentType.Quiz;
    private decimal _newAssessmentMaxScore = 50;

    public ClassesViewModel(DataService dataService, NotificationService notificationService)
    {
        _dataService = dataService;
        _notifications = notificationService;
        Classes = dataService.Classes;
        Subjects = Enum.GetValues<Subject>();
        AssessmentTypes = Enum.GetValues<AssessmentType>();
        SelectedClass = Classes.FirstOrDefault();

        CreateClassCommand = new RelayCommandAsync(CreateClassAsync, () => !string.IsNullOrWhiteSpace(NewClassName), ex => _notifications.ShowError(UnwrapMessage(ex)));
        DeleteClassCommand = new RelayCommandAsync(DeleteClassAsync, () => SelectedClass is not null, ex => _notifications.ShowError(UnwrapMessage(ex)));
        AddStudentCommand = new RelayCommandAsync(AddStudentAsync, () => SelectedClass is not null && !string.IsNullOrWhiteSpace(NewStudentName), ex => _notifications.ShowError(UnwrapMessage(ex)));
        DeleteStudentCommand = new RelayCommand(DeleteStudent, onError: ex => _notifications.ShowError(UnwrapMessage(ex)));
        MarkPresentCommand = new RelayCommand(parameter => MarkAttendance(parameter, AttendanceStatus.Present), onError: ex => _notifications.ShowError(UnwrapMessage(ex)));
        MarkLateCommand = new RelayCommand(parameter => MarkAttendance(parameter, AttendanceStatus.Late), onError: ex => _notifications.ShowError(UnwrapMessage(ex)));
        MarkAbsentCommand = new RelayCommand(parameter => MarkAttendance(parameter, AttendanceStatus.Absent), onError: ex => _notifications.ShowError(UnwrapMessage(ex)));
        MarkExcusedCommand = new RelayCommand(parameter => MarkAttendance(parameter, AttendanceStatus.Excused), onError: ex => _notifications.ShowError(UnwrapMessage(ex)));
        AddAssessmentCommand = new RelayCommandAsync(AddAssessmentAsync, () => SelectedClass is not null && !string.IsNullOrWhiteSpace(NewAssessmentName) && NewAssessmentMaxScore > 0, ex => _notifications.ShowError(UnwrapMessage(ex)));
        DeleteAssessmentCommand = new RelayCommand(DeleteAssessment, onError: ex => _notifications.ShowError(UnwrapMessage(ex)));
        UploadContentCommand = new RelayCommandAsync(UploadContentAsync, () => SelectedClass is not null, ex => _notifications.ShowError(UnwrapMessage(ex)));
        DeleteContentCommand = new RelayCommand(DeleteContent, onError: ex => _notifications.ShowError(UnwrapMessage(ex)));
        OpenContentCommand = new RelayCommand(OpenContent, onError: ex => _notifications.ShowError(UnwrapMessage(ex)));

        _dataService.Students.CollectionChanged += OnSharedClassDataChanged;
        _dataService.Assessments.CollectionChanged += OnSharedClassDataChanged;
        _dataService.ContentItems.CollectionChanged += OnSharedClassDataChanged;
        _dataService.Attendance.CollectionChanged += OnSharedClassDataChanged;
    }

    public ObservableCollection<Class> Classes { get; }
    public ObservableCollection<StudentRosterViewModel> Students { get; } = [];
    public ObservableCollection<AttendanceStudentViewModel> AttendanceRows { get; } = [];
    public ObservableCollection<Assessment> Assessments { get; } = [];
    public ObservableCollection<GradeBookRowViewModel> GradeRows { get; } = [];
    public ObservableCollection<ContentItem> ContentItems { get; } = [];
    public IReadOnlyList<Subject> Subjects { get; }
    public IReadOnlyList<AssessmentType> AssessmentTypes { get; }

    public ICommand CreateClassCommand { get; }
    public ICommand DeleteClassCommand { get; }
    public ICommand AddStudentCommand { get; }
    public ICommand DeleteStudentCommand { get; }
    public ICommand MarkPresentCommand { get; }
    public ICommand MarkLateCommand { get; }
    public ICommand MarkAbsentCommand { get; }
    public ICommand MarkExcusedCommand { get; }
    public ICommand AddAssessmentCommand { get; }
    public ICommand DeleteAssessmentCommand { get; }
    public ICommand UploadContentCommand { get; }
    public ICommand DeleteContentCommand { get; }
    public ICommand OpenContentCommand { get; }

    public Class? SelectedClass
    {
        get => _selectedClass;
        set
        {
            if (SetProperty(ref _selectedClass, value))
            {
                OnPropertyChanged(nameof(SelectedWeights));
                OnPropertyChanged(nameof(WrittenWorksPercent));
                OnPropertyChanged(nameof(PerformanceTasksPercent));
                OnPropertyChanged(nameof(QuarterlyAssessmentPercent));
                RefreshClassWorkspace();
                RaiseCommandStates();
            }
        }
    }

    public string NewClassName
    {
        get => _newClassName;
        set
        {
            if (SetProperty(ref _newClassName, value))
            {
                RaiseCommandStates();
            }
        }
    }

    public string NewClassSection
    {
        get => _newClassSection;
        set => SetProperty(ref _newClassSection, value);
    }

    public Subject SelectedSubject
    {
        get => _selectedSubject;
        set => SetProperty(ref _selectedSubject, value);
    }

    public string ClassSearch
    {
        get => _classSearch;
        set
        {
            if (SetProperty(ref _classSearch, value))
            {
                RefreshClassWorkspace();
            }
        }
    }

    public string NewStudentName
    {
        get => _newStudentName;
        set
        {
            if (SetProperty(ref _newStudentName, value))
            {
                RaiseCommandStates();
            }
        }
    }

    public DateTime AttendanceDate
    {
        get => _attendanceDate;
        set
        {
            if (SetProperty(ref _attendanceDate, value))
            {
                RefreshAttendance();
            }
        }
    }

    public string NewAssessmentName
    {
        get => _newAssessmentName;
        set
        {
            if (SetProperty(ref _newAssessmentName, value))
            {
                RaiseCommandStates();
            }
        }
    }

    public AssessmentType NewAssessmentType
    {
        get => _newAssessmentType;
        set => SetProperty(ref _newAssessmentType, value);
    }

    public decimal NewAssessmentMaxScore
    {
        get => _newAssessmentMaxScore;
        set
        {
            var clamped = Math.Clamp(value, 1, 1000);
            if (SetProperty(ref _newAssessmentMaxScore, clamped))
            {
                RaiseCommandStates();
            }
        }
    }

    public GradingConfig SelectedWeights => GradingConfig.FromSubject(SelectedClass?.Subject ?? SelectedSubject);
    public string WrittenWorksPercent => $"{SelectedWeights.WrittenWorksWeight:P0}";
    public string PerformanceTasksPercent => $"{SelectedWeights.PerformanceTasksWeight:P0}";
    public string QuarterlyAssessmentPercent => $"{SelectedWeights.QuarterlyAssessmentWeight:P0}";

    private async Task CreateClassAsync()
    {
        var colors = new[] { "#2C5EAD", "#1591DC", "#0C7779", "#249E94", "#3BC1A8" };
        var created = await _dataService.AddClassAsync(new Class
        {
            Name = NewClassName.Trim(),
            Section = string.IsNullOrWhiteSpace(NewClassSection) ? "Section" : NewClassSection.Trim(),
            Subject = SelectedSubject,
            SchoolYear = DateTime.Today.Year.ToString(),
            Semester = "1st",
            CoverColor = colors[Classes.Count % colors.Length]
        });

        NewClassName = string.Empty;
        NewClassSection = string.Empty;
        SelectedClass = created;
    }

    private async Task DeleteClassAsync()
    {
        if (SelectedClass is null)
        {
            return;
        }

        var result = MessageBox.Show($"Delete {SelectedClass.Name}? This removes it from the class list.", "Delete class", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (result != MessageBoxResult.Yes)
        {
            return;
        }

        var deleted = SelectedClass;
        await _dataService.DeleteClassAsync(deleted);
        SelectedClass = Classes.FirstOrDefault();
    }

    private async Task AddStudentAsync()
    {
        if (SelectedClass is null)
        {
            return;
        }

        await _dataService.AddStudentAsync(new Student
        {
            FullName = NewStudentName.Trim(),
            StudentId = $"STU-{DateTime.Now:HHmmss}",
            ClassId = SelectedClass.Id
        });

        NewStudentName = string.Empty;
        RefreshClassWorkspace();
    }

    private async void DeleteStudent(object? parameter)
    {
        try
        {
            if (parameter is not StudentRosterViewModel row)
            {
                return;
            }

            var result = MessageBox.Show($"Delete {row.Student.FullName} from the roster?", "Delete student", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            await _dataService.DeleteStudentAsync(row.Student);
            RefreshClassWorkspace();
        }
        catch (Exception ex)
        {
            _notifications.ShowError(UnwrapMessage(ex));
        }
    }

    private async void MarkAttendanceAsync(AttendanceStudentViewModel row, AttendanceStatus status)
    {
        try
        {
            await _dataService.SaveAttendanceRecordAsync(row.Student.Id, AttendanceDate, status);
            RefreshRosterAttendanceCounts();
        }
        catch (Exception ex)
        {
            _notifications.ShowError(UnwrapMessage(ex));
        }
    }

    private void MarkAttendance(object? parameter, AttendanceStatus status)
    {
        if (parameter is not AttendanceStudentViewModel row)
        {
            return;
        }

        row.Status = status;
        MarkAttendanceAsync(row, status);
    }

    private async Task AddAssessmentAsync()
    {
        if (SelectedClass is null)
        {
            return;
        }

        await _dataService.AddAssessmentAsync(new Assessment
        {
            ClassId = SelectedClass.Id,
            Name = NewAssessmentName.Trim(),
            Type = NewAssessmentType,
            MaxScore = NewAssessmentMaxScore,
            Weight = NewAssessmentType switch
            {
                AssessmentType.Quiz => SelectedWeights.WrittenWorksWeight,
                AssessmentType.Activity => SelectedWeights.PerformanceTasksWeight,
                _ => SelectedWeights.QuarterlyAssessmentWeight
            },
            Date = DateTime.Today
        });

        NewAssessmentName = string.Empty;
        RefreshClassWorkspace();
    }

    private async void DeleteAssessment(object? parameter)
    {
        try
        {
            if (parameter is not Assessment assessment)
            {
                return;
            }

            var result = MessageBox.Show($"Delete \"{assessment.Name}\"? This removes all scores for this assessment.", "Delete assessment", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            await _dataService.DeleteAssessmentAsync(assessment);
            RefreshClassWorkspace();
        }
        catch (Exception ex)
        {
            _notifications.ShowError(UnwrapMessage(ex));
        }
    }

    private async Task UploadContentAsync()
    {
        if (SelectedClass is null)
        {
            return;
        }

        var dialog = new OpenFileDialog
        {
            Title = "Upload class content",
            Filter = "Supported files|*.pdf;*.doc;*.docx;*.ppt;*.pptx;*.png;*.jpg;*.jpeg;*.mp4;*.mov|All files|*.*"
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        var extension = Path.GetExtension(dialog.FileName).ToLowerInvariant();
        await _dataService.AddContentItemAsync(new ContentItem
        {
            ClassId = SelectedClass.Id,
            Title = Path.GetFileNameWithoutExtension(dialog.FileName),
            Description = dialog.FileName,
            Type = extension switch
            {
                ".pdf" => ContentType.Pdf,
                ".doc" or ".docx" => ContentType.Doc,
                ".ppt" or ".pptx" => ContentType.Ppt,
                ".png" or ".jpg" or ".jpeg" => ContentType.Image,
                ".mp4" or ".mov" => ContentType.Video,
                _ => ContentType.Link
            },
            FilePath = dialog.FileName,
            UploadedAt = DateTime.Now,
            IsVisible = true
        });

        RefreshClassWorkspace();
    }

    private async void DeleteContent(object? parameter)
    {
        try
        {
            if (parameter is not ContentItem content)
            {
                return;
            }

            await _dataService.DeleteContentItemAsync(content);
            ContentItems.Remove(content);
        }
        catch (Exception ex)
        {
            _notifications.ShowError(UnwrapMessage(ex));
        }
    }

    private void OpenContent(object? parameter)
    {
        try
        {
            if (parameter is not ContentItem content || string.IsNullOrWhiteSpace(content.FilePath))
            {
                return;
            }

            if (!File.Exists(content.FilePath))
            {
                _notifications.ShowInfo("The file could not be found on this device.");
                return;
            }

            Process.Start(new ProcessStartInfo(content.FilePath) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            _notifications.ShowError(UnwrapMessage(ex));
        }
    }

    private void RefreshClassWorkspace()
    {
        Students.Clear();
        Assessments.Clear();
        GradeRows.Clear();
        ContentItems.Clear();

        if (SelectedClass is null)
        {
            return;
        }

        foreach (var student in _dataService.GetStudentsForClass(SelectedClass.Id)
            .Where(item => MatchesFilter(item))
            .OrderBy(item => item.FullName))
        {
            Students.Add(new StudentRosterViewModel(student, _dataService.Attendance.ToList()));
        }

        foreach (var assessment in _dataService.GetAssessmentsForClass(SelectedClass.Id).OrderBy(item => item.Date).ThenBy(item => item.Id))
        {
            Assessments.Add(assessment);
        }

        foreach (var content in _dataService.GetContentForClass(SelectedClass.Id).OrderByDescending(item => item.UploadedAt))
        {
            ContentItems.Add(content);
        }

        RefreshGrades();
        RefreshAttendance();
    }

    private bool MatchesFilter(Student student)
    {
        var search = string.IsNullOrWhiteSpace(ClassSearch) ? null : ClassSearch;
        return search is null || MatchesStudent(student, search);
    }

    private void RefreshRosterAttendanceCounts()
    {
        var allRecords = _dataService.Attendance.ToList();
        foreach (var row in Students)
        {
            row.ComputeAttendance(allRecords);
        }
    }

    private static bool MatchesStudent(Student student, string searchText) =>
        student.FullName.Contains(searchText, StringComparison.OrdinalIgnoreCase);

    private void OnSharedClassDataChanged(object? sender, NotifyCollectionChangedEventArgs e) => RefreshClassWorkspace();

    private void RefreshGrades()
    {
        GradeRows.Clear();
        foreach (var rowVm in Students)
        {
            var student = rowVm.Student;
            var row = new GradeBookRowViewModel { Student = student };
            foreach (var assessment in Assessments)
            {
                var score = _dataService.Scores.FirstOrDefault(item => item.StudentId == student.Id && item.AssessmentId == assessment.Id);
                row.Cells.Add(new GradeCellViewModel(assessment, score?.Value, async cell =>
                {
                    try
                    {
                        decimal? clamped = cell.Value.HasValue ? Math.Clamp(cell.Value.Value, 0, cell.MaxScore) : null;
                        await _dataService.SaveScoreAsync(row.Student.Id, cell.AssessmentId, clamped);
                        RecomputeGrade(row);
                    }
                    catch (Exception ex)
                    {
                        _notifications.ShowError(UnwrapMessage(ex));
                    }
                }));
            }

            RecomputeGrade(row);
            GradeRows.Add(row);
        }
    }

    private void RecomputeGrade(GradeBookRowViewModel row)
    {
        row.WrittenWorksAverage = AverageCategory(row, AssessmentType.Quiz);
        row.PerformanceTasksAverage = AverageCategory(row, AssessmentType.Activity);
        row.QuarterlyAssessmentAverage = AverageCategory(row, AssessmentType.Exam);
        row.InitialGrade = Math.Round((row.WrittenWorksAverage * SelectedWeights.WrittenWorksWeight) + (row.PerformanceTasksAverage * SelectedWeights.PerformanceTasksWeight) + (row.QuarterlyAssessmentAverage * SelectedWeights.QuarterlyAssessmentWeight), 2);
    }

    private static decimal AverageCategory(GradeBookRowViewModel row, AssessmentType type)
    {
        var percentages = row.Cells
            .Where(cell => cell.Type == type && cell.Value.HasValue && cell.MaxScore > 0)
            .Select(cell => cell.Value!.Value / cell.MaxScore * 100)
            .ToList();

        return percentages.Count == 0 ? 0 : Math.Round(percentages.Average(), 2);
    }

    private void RefreshAttendance()
    {
        AttendanceRows.Clear();
        foreach (var rowVm in Students)
        {
            var student = rowVm.Student;
            var existing = _dataService.Attendance.FirstOrDefault(item => item.StudentId == student.Id && item.Date.Date == AttendanceDate.Date);
            AttendanceRows.Add(new AttendanceStudentViewModel(student, existing?.Status ?? AttendanceStatus.Present));
        }
    }

    private static string UnwrapMessage(Exception ex) => ex is ServiceException se ? se.UserFacingMessage : "Something went wrong. Please try again.";

    private void RaiseCommandStates()
    {
        (CreateClassCommand as RelayCommandAsync)?.RaiseCanExecuteChanged();
        (DeleteClassCommand as RelayCommandAsync)?.RaiseCanExecuteChanged();
        (AddStudentCommand as RelayCommandAsync)?.RaiseCanExecuteChanged();
        (AddAssessmentCommand as RelayCommandAsync)?.RaiseCanExecuteChanged();
        (DeleteAssessmentCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (UploadContentCommand as RelayCommandAsync)?.RaiseCanExecuteChanged();
    }
}
