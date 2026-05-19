using Instrux.Application.Helpers;
using Instrux.Domain.Enums;
using Instrux.Domain.Models;

namespace Instrux.Application.ViewModels;

public sealed class AttendanceStudentViewModel : ViewModelBase
{
    private AttendanceStatus _status;

    public AttendanceStudentViewModel(Student student, AttendanceStatus status)
    {
        Student = student;
        _status = status;
    }

    public Student Student { get; }

    public AttendanceStatus Status
    {
        get => _status;
        set
        {
            if (SetProperty(ref _status, value))
            {
                OnPropertyChanged(nameof(PresentMarker));
                OnPropertyChanged(nameof(LateMarker));
                OnPropertyChanged(nameof(AbsentMarker));
                OnPropertyChanged(nameof(ExcusedMarker));
            }
        }
    }

    public string PresentMarker => Status == AttendanceStatus.Present ? "● Present" : "○ Present";
    public string LateMarker => Status == AttendanceStatus.Late ? "● Late" : "○ Late";
    public string AbsentMarker => Status == AttendanceStatus.Absent ? "● Absent" : "○ Absent";
    public string ExcusedMarker => Status == AttendanceStatus.Excused ? "● Excused" : "○ Excused";
}
