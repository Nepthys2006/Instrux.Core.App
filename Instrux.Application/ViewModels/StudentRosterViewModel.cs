using Instrux.Application.Helpers;
using Instrux.Domain.Enums;
using Instrux.Domain.Models;

namespace Instrux.Application.ViewModels;

public sealed class StudentRosterViewModel : ViewModelBase
{
    public Student Student { get; }

    public int TotalPresent { get; private set; }
    public int TotalLate { get; private set; }
    public int TotalAbsences { get; private set; }
    public int TotalExcused { get; private set; }
    public int TotalAttendance { get; private set; }

    public StudentRosterViewModel(Student student, IEnumerable<AttendanceRecord> attendanceRecords)
    {
        Student = student;
        ComputeAttendance(attendanceRecords);
    }

    public void ComputeAttendance(IEnumerable<AttendanceRecord> attendanceRecords)
    {
        var records = attendanceRecords.Where(r => r.StudentId == Student.Id).ToList();
        TotalPresent = records.Count(r => r.Status == AttendanceStatus.Present);
        TotalLate = records.Count(r => r.Status == AttendanceStatus.Late);
        TotalAbsences = records.Count(r => r.Status == AttendanceStatus.Absent);
        TotalExcused = records.Count(r => r.Status == AttendanceStatus.Excused);
        TotalAttendance = records.Count;

        OnPropertyChanged(nameof(TotalPresent));
        OnPropertyChanged(nameof(TotalLate));
        OnPropertyChanged(nameof(TotalAbsences));
        OnPropertyChanged(nameof(TotalExcused));
        OnPropertyChanged(nameof(TotalAttendance));
    }
}
