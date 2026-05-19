using System.Windows;
using System.Windows.Input;
using Instrux.Application.ViewModels;

namespace Instrux.Application;

public partial class AuthenticationWindow : Window
{
    public AuthenticationWindow(AuthenticationViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        viewModel.AuthenticationSucceeded += (_, _) =>
        {
            DialogResult = true;
            Close();
        };
    }

    private void OnDragMove(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed)
        {
            DragMove();
        }
    }
}
