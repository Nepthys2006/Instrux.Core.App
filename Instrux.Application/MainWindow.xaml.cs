using System.Windows;
using Instrux.Application.ViewModels;

namespace Instrux.Application;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public event EventHandler? SignOutRequested;

    public MainWindow(MainDashboardViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        viewModel.SignOutRequested += OnSignOutRequested;
    }

    private void OnSignOutRequested(object? sender, EventArgs e)
    {
        SignOutRequested?.Invoke(this, EventArgs.Empty);
        Close();
    }
}
