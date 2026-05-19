namespace Instrux.Domain.Models;

public class Score
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int AssessmentId { get; set; }
    public decimal? Value { get; set; }
}
