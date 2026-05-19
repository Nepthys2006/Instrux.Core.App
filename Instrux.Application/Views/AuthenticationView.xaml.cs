using System;

namespace Instrux.Application.Views;

public partial class AuthenticationView
{
    public AuthenticationView()
    {
        InitializeComponent();
    }

    private void OnPasswordChanged(object sender, System.Windows.RoutedEventArgs e)
    {
        try
        {
            if (DataContext is ViewModels.AuthenticationViewModel viewModel)
            {
                viewModel.Password = PasswordInput.Password;
            }
        }
        catch
        {
            // Silently handle if DataContext is disposed
        }
    }
}
