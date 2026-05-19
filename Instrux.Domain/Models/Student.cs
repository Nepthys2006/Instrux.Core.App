namespace Instrux.Domain.Models;

public class Student
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string StudentId { get; set; } = string.Empty;
    public string? Email { get; set; }
    public int ClassId { get; set; }
}
