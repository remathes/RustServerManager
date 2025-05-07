
// SnackbarService.cs
using MaterialDesignThemes.Wpf;
using System;

public static class SnackbarService
{
    public static ISnackbarMessageQueue MessageQueue { get; set; }

    public static void Show(string message, int durationSeconds = 3)
    {
        MessageQueue?.Enqueue(message, null, null, null, false, true, TimeSpan.FromSeconds(durationSeconds));
    }
}
