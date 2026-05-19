using Instrux.Domain.Enums;

namespace Instrux.Domain.Models;

public class Class
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Section { get; set; }
    public Subject Subject { get; set; }
    public string? SchoolYear { get; set; }
    public string? Semester { get; set; }
    public string CoverColor { get; set; } = "#2563EB";
    public int TeacherId { get; set; }
}
