using System.Windows.Input;
using Instrux.Application.Helpers;
using Instrux.Application.Services;
using Instrux.Services.DTOs;
using Instrux.Services.Interfaces;

namespace Instrux.Application.ViewModels;

public sealed class AuthenticationViewModel : ViewModelBase
{
    private readonly IAuthenticationService _authenticationService;
    private readonly SessionService _sessionService;
    private string _email = string.Empty;
    private string _password = string.Empty;
    private string _fullName = string.Empty;
    private string _nickname = string.Empty;
    private string _errorMessage = string.Empty;
    private bool _isSignUp;

    public AuthenticationViewModel(IAuthenticationService authenticationService, SessionService sessionService)
    {
        _authenticationService = authenticationService;
        _sessionService = sessionService;
        SubmitCommand = new RelayCommandAsync(SubmitAsync, CanSubmit);
        ToggleModeCommand = new RelayCommand(ToggleMode);
    }

    public event EventHandler? AuthenticationSucceeded;

    public ICommand SubmitCommand { get; }
    public ICommand ToggleModeCommand { get; }

    public string Email
    {
        get => _email;
        set
        {
            if (SetProperty(ref _email, value))
            {
                RaiseSubmitCanExecuteChanged();
            }
        }
    }

    public string Password
    {
        get => _password;
        set
        {
            if (SetProperty(ref _password, value))
            {
                RaiseSubmitCanExecuteChanged();
            }
        }
    }

    public string FullName
    {
        get => _fullName;
        set
        {
            if (SetProperty(ref _fullName, value))
            {
                RaiseSubmitCanExecuteChanged();
            }
        }
    }

    public string Nickname
    {
        get => _nickname;
        set => SetProperty(ref _nickname, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set
        {
            if (SetProperty(ref _errorMessage, value))
            {
                OnPropertyChanged(nameof(HasError));
            }
        }
    }

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    public bool IsSignUp
    {
        get => _isSignUp;
        set
        {
            if (SetProperty(ref _isSignUp, value))
            {
                OnPropertyChanged(nameof(Title));
                OnPropertyChanged(nameof(Subtitle));
                OnPropertyChanged(nameof(PrimaryActionText));
                OnPropertyChanged(nameof(ToggleActionText));
                RaiseSubmitCanExecuteChanged();
            }
        }
    }

    public string Title => IsSignUp ? "Create your workspace" : "Welcome back";
    public string Subtitle => IsSignUp ? "Start managing your classes with Instrux." : "Sign in to your teacher command center.";
    public string PrimaryActionText => IsSignUp ? "Create account" : "Sign in";
    public string ToggleActionText => IsSignUp ? "Already have an account? Sign in" : "New to Instrux? Create account";

    private bool CanSubmit() => !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(Password) && (!IsSignUp || !string.IsNullOrWhiteSpace(FullName));

    private async Task SubmitAsync()
    {
        ErrorMessage = string.Empty;
        var result = IsSignUp
            ? await _authenticationService.RegisterAsync(new RegisterRequestDto(FullName, string.IsNullOrWhiteSpace(Nickname) ? FullName : Nickname, Email, Password))
            : await _authenticationService.LoginAsync(new LoginRequestDto(Email, Password));

        if (!result.Success || result.Teacher is null)
        {
            ErrorMessage = result.Message;
            return;
        }

        _sessionService.SignIn(result.Teacher);
        AuthenticationSucceeded?.Invoke(this, EventArgs.Empty);
    }

    private void ToggleMode()
    {
        ErrorMessage = string.Empty;
        IsSignUp = !IsSignUp;
    }

    private void RaiseSubmitCanExecuteChanged()
    {
        if (SubmitCommand is RelayCommandAsync command)
        {
            command.RaiseCanExecuteChanged();
        }
    }
}
