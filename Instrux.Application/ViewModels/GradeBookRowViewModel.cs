using System.Collections.ObjectModel;
using Instrux.Application.Helpers;
using Instrux.Domain.Enums;
using Instrux.Domain.Models;

namespace Instrux.Application.ViewModels;

public sealed class GradeBookRowViewModel : ViewModelBase
{
    private decimal _writtenWorksAverage;
    private decimal _performanceTasksAverage;
    private decimal _quarterlyAssessmentAverage;
    private decimal _initialGrade;

    public required Student Student { get; init; }
    public ObservableCollection<GradeCellViewModel> Cells { get; } = [];

    public decimal WrittenWorksAverage
    {
        get => _writtenWorksAverage;
        set => SetProperty(ref _writtenWorksAverage, value);
    }

    public decimal PerformanceTasksAverage
    {
        get => _performanceTasksAverage;
        set => SetProperty(ref _performanceTasksAverage, value);
    }

    public decimal QuarterlyAssessmentAverage
    {
        get => _quarterlyAssessmentAverage;
        set => SetProperty(ref _quarterlyAssessmentAverage, value);
    }

    public decimal InitialGrade
    {
        get => _initialGrade;
        set
        {
            if (SetProperty(ref _initialGrade, value))
            {
                OnPropertyChanged(nameof(Standing));
            }
        }
    }

    public string Standing => InitialGrade >= 90 ? "Excellent" : InitialGrade >= 80 ? "On track" : InitialGrade >= 70 ? "Watch" : "Support";
}

public sealed class GradeCellViewModel : ViewModelBase
{
    private decimal? _value;
    private readonly Func<GradeCellViewModel, Task> _save;

    public GradeCellViewModel(Assessment assessment, decimal? value, Func<GradeCellViewModel, Task> save)
    {
        Assessment = assessment;
        _value = value;
        _save = save;
    }

    public Assessment Assessment { get; }
    public int AssessmentId => Assessment.Id;
    public AssessmentType Type => Assessment.Type;
    public decimal MaxScore => Assessment.MaxScore;

    public decimal? Value
    {
        get => _value;
        set
        {
            if (SetProperty(ref _value, value))
            {
                _ = SaveScoreAsync();
            }
        }
    }

    private async Task SaveScoreAsync()
    {
        await _save(this);
    }
}
