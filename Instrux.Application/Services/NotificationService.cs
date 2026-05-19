using MaterialDesignThemes.Wpf;

namespace Instrux.Application.Services;

public sealed class NotificationService
{
    public ISnackbarMessageQueue MessageQueue { get; } = new SnackbarMessageQueue(TimeSpan.FromSeconds(5));

    public void ShowError(string message) => MessageQueue.Enqueue(message, "OK", null);
    public void ShowSuccess(string message) => MessageQueue.Enqueue(message);
    public void ShowInfo(string message) => MessageQueue.Enqueue(message);
}
