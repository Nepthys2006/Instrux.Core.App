using Instrux.Domain.Enums;

namespace Instrux.Domain.Models;

public class GradingConfig
{
    public int Id { get; set; }
    public Subject Subject { get; set; }
    public SubjectGroup Group { get; set; }
    public decimal WrittenWorksWeight { get; set; }
    public decimal PerformanceTasksWeight { get; set; }
    public decimal QuarterlyAssessmentWeight { get; set; }

    public static GradingConfig FromSubject(Subject subject) => subject switch
    {
        Subject.English or Subject.Filipino or Subject.AralingPanlipunan or Subject.EdukasyonSaPagpapakatao
            => new()
            {
                Subject = subject,
                Group = SubjectGroup.LanguagesSocialSciences,
                WrittenWorksWeight = 0.30m,
                PerformanceTasksWeight = 0.50m,
                QuarterlyAssessmentWeight = 0.20m
            },
        Subject.Mathematics or Subject.Science
            => new()
            {
                Subject = subject,
                Group = SubjectGroup.MathScience,
                WrittenWorksWeight = 0.40m,
                PerformanceTasksWeight = 0.40m,
                QuarterlyAssessmentWeight = 0.20m
            },
        Subject.TLE or Subject.HomeEconomics or Subject.MAPEH
            => new()
            {
                Subject = subject,
                Group = SubjectGroup.SkillsArts,
                WrittenWorksWeight = 0.20m,
                PerformanceTasksWeight = 0.60m,
                QuarterlyAssessmentWeight = 0.20m
            },
        _ => throw new ArgumentOutOfRangeException(nameof(subject))
    };
}
