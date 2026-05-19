using Instrux.Application.Helpers;

namespace Instrux.Application.ViewModels;

public sealed class NavigationItemViewModel : ViewModelBase
{
    private bool _isSelected;

    public required string Title { get; init; }
    public required string IconPath { get; init; }
    public required object Page { get; init; }

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }
}
