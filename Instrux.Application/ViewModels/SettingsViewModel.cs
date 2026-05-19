using System.Windows;
using System.Windows.Input;
using Instrux.Application.Helpers;
using Instrux.Application.Services;
using Instrux.Services.DTOs;
using Instrux.Services.Exceptions;
using Instrux.Services.Interfaces;

namespace Instrux.Application.ViewModels;

public sealed class SettingsViewModel : ViewModelBase
{
    private readonly SessionService _sessionService;
    private readonly ITeacherService _teacherService;
    private readonly NotificationService _notifications;
    private bool _isEditing;
    private string _fullName;
    private string _nickname;
    private string _email;

    public SettingsViewModel(SessionService sessionService, ITeacherService teacherService, NotificationService notificationService)
    {
        _sessionService = sessionService;
        _teacherService = teacherService;
        _notifications = notificationService;
        _fullName = sessionService.CurrentTeacher.FullName;
        _nickname = sessionService.CurrentTeacher.Nickname;
        _email = sessionService.CurrentTeacher.Email;
        ToggleEditingCommand = new RelayCommand(() => IsEditing = !IsEditing, onError: ex => _notifications.ShowError(UnwrapMessage(ex)));
        SaveCommand = new RelayCommandAsync(SaveAsync, () => IsEditing && !string.IsNullOrWhiteSpace(FullName) && !string.IsNullOrWhiteSpace(Nickname) && !string.IsNullOrWhiteSpace(Email), ex => _notifications.ShowError(UnwrapMessage(ex)));
        DeleteAccountCommand = new RelayCommandAsync(DeleteAccountAsync, onError: ex => _notifications.ShowError(UnwrapMessage(ex)));
    }

    public event EventHandler? SignOutRequested;

    public ICommand ToggleEditingCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand DeleteAccountCommand { get; }

    public string FullName
    {
        get => _fullName;
        set
        {
            if (SetProperty(ref _fullName, value))
            {
                (SaveCommand as RelayCommandAsync)?.RaiseCanExecuteChanged();
            }
        }
    }

    public string Nickname
    {
        get => _nickname;
        set
        {
            if (SetProperty(ref _nickname, value))
            {
                (SaveCommand as RelayCommandAsync)?.RaiseCanExecuteChanged();
            }
        }
    }

    public string Email
    {
        get => _email;
        set
        {
            if (SetProperty(ref _email, value))
            {
                (SaveCommand as RelayCommandAsync)?.RaiseCanExecuteChanged();
            }
        }
    }

    public bool IsEditing
    {
        get => _isEditing;
        set
        {
            if (SetProperty(ref _isEditing, value))
            {
                _notifications.ShowInfo(IsEditing ? "Profile unlocked" : "Profile locked");
                OnPropertyChanged(nameof(LockButtonText));
                (SaveCommand as RelayCommandAsync)?.RaiseCanExecuteChanged();
            }
        }
    }

    public string LockButtonText => IsEditing ? "Lock profile" : "Unlock profile";

    private async Task SaveAsync()
    {
        try
        {
            var saved = await _teacherService.UpdateProfileAsync(new TeacherDto(_sessionService.CurrentTeacher.Id, FullName.Trim(), Nickname.Trim(), Email.Trim()));
            _sessionService.UpdateCurrentTeacher(saved);
            FullName = saved.FullName;
            Nickname = saved.Nickname;
            Email = saved.Email;
            IsEditing = false;
            _notifications.ShowSuccess("Profile saved");
        }
        catch (Exception ex)
        {
            _notifications.ShowError(UnwrapMessage(ex));
        }
    }

    private async Task DeleteAccountAsync()
    {
        try
        {
            var result = MessageBox.Show("Permanently delete your account and all associated data? This cannot be undone.", "Delete account", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            await _teacherService.DeleteAccountAsync(_sessionService.CurrentTeacher.Id);
            _sessionService.SignOut();
            SignOutRequested?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            _notifications.ShowError(UnwrapMessage(ex));
        }
    }

    private static string UnwrapMessage(Exception ex) => ex is ServiceException se ? se.UserFacingMessage : "Something went wrong. Please try again.";
}
