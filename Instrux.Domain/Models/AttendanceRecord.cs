using Instrux.Domain.Enums;

namespace Instrux.Domain.Models;

public class AttendanceRecord
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public DateTime Date { get; set; }
    public AttendanceStatus Status { get; set; }
    public string? Note { get; set; }
}
