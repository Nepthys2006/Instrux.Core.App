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
            try
            {
                DialogResult = true;
                Close();
            }
            catch
            {
                // Silently handle if already closed
            }
        };
    }

    private void OnDragMove(object sender, MouseButtonEventArgs e)
    {
        try
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
        catch
        {
            // Silently handle drag move failures
        }
    }
}
