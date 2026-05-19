using Instrux.Domain.Enums;

namespace Instrux.Domain.Models;

public class Assessment
{
    public int Id { get; set; }
    public int ClassId { get; set; }
    public string Name { get; set; } = string.Empty;
    public AssessmentType Type { get; set; }
    public decimal MaxScore { get; set; }
    public decimal Weight { get; set; }
    public DateTime Date { get; set; }
}
