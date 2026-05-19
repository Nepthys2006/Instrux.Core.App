using Instrux.Domain.Enums;
using Instrux.Domain.Models;

namespace Instrux.Tests;

public sealed class GradingConfigTests
{
    [Fact]
    public void English_Group_Returns_30_50_20()
    {
        var config = GradingConfig.FromSubject(Subject.English);

        Assert.Equal(0.30m, config.WrittenWorksWeight);
        Assert.Equal(0.50m, config.PerformanceTasksWeight);
        Assert.Equal(0.20m, config.QuarterlyAssessmentWeight);
        Assert.Equal(SubjectGroup.LanguagesSocialSciences, config.Group);
    }

    [Fact]
    public void Filipino_Group_Returns_30_50_20()
    {
        var config = GradingConfig.FromSubject(Subject.Filipino);

        Assert.Equal(0.30m, config.WrittenWorksWeight);
        Assert.Equal(0.50m, config.PerformanceTasksWeight);
        Assert.Equal(0.20m, config.QuarterlyAssessmentWeight);
    }

    [Fact]
    public void AralingPanlipunan_Group_Returns_30_50_20()
    {
        var config = GradingConfig.FromSubject(Subject.AralingPanlipunan);

        Assert.Equal(0.30m, config.WrittenWorksWeight);
        Assert.Equal(0.50m, config.PerformanceTasksWeight);
        Assert.Equal(0.20m, config.QuarterlyAssessmentWeight);
    }

    [Fact]
    public void EdukasyonSaPagpapakatao_Group_Returns_30_50_20()
    {
        var config = GradingConfig.FromSubject(Subject.EdukasyonSaPagpapakatao);

        Assert.Equal(0.30m, config.WrittenWorksWeight);
        Assert.Equal(0.50m, config.PerformanceTasksWeight);
        Assert.Equal(0.20m, config.QuarterlyAssessmentWeight);
    }

    [Fact]
    public void Mathematics_Group_Returns_40_40_20()
    {
        var config = GradingConfig.FromSubject(Subject.Mathematics);

        Assert.Equal(0.40m, config.WrittenWorksWeight);
        Assert.Equal(0.40m, config.PerformanceTasksWeight);
        Assert.Equal(0.20m, config.QuarterlyAssessmentWeight);
        Assert.Equal(SubjectGroup.MathScience, config.Group);
    }

    [Fact]
    public void Science_Group_Returns_40_40_20()
    {
        var config = GradingConfig.FromSubject(Subject.Science);

        Assert.Equal(0.40m, config.WrittenWorksWeight);
        Assert.Equal(0.40m, config.PerformanceTasksWeight);
        Assert.Equal(0.20m, config.QuarterlyAssessmentWeight);
    }

    [Fact]
    public void TLE_Group_Returns_20_60_20()
    {
        var config = GradingConfig.FromSubject(Subject.TLE);

        Assert.Equal(0.20m, config.WrittenWorksWeight);
        Assert.Equal(0.60m, config.PerformanceTasksWeight);
        Assert.Equal(0.20m, config.QuarterlyAssessmentWeight);
        Assert.Equal(SubjectGroup.SkillsArts, config.Group);
    }

    [Fact]
    public void HomeEconomics_Group_Returns_20_60_20()
    {
        var config = GradingConfig.FromSubject(Subject.HomeEconomics);

        Assert.Equal(0.20m, config.WrittenWorksWeight);
        Assert.Equal(0.60m, config.PerformanceTasksWeight);
        Assert.Equal(0.20m, config.QuarterlyAssessmentWeight);
    }

    [Fact]
    public void MAPEH_Group_Returns_20_60_20()
    {
        var config = GradingConfig.FromSubject(Subject.MAPEH);

        Assert.Equal(0.20m, config.WrittenWorksWeight);
        Assert.Equal(0.60m, config.PerformanceTasksWeight);
        Assert.Equal(0.20m, config.QuarterlyAssessmentWeight);
    }
}
