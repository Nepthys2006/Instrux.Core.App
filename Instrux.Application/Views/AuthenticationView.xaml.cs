namespace Instrux.Application.Views;

public partial class AuthenticationView
{
    public AuthenticationView()
    {
        InitializeComponent();
    }

    private void OnPasswordChanged(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is ViewModels.AuthenticationViewModel viewModel)
        {
            viewModel.Password = PasswordInput.Password;
        }
    }
}
