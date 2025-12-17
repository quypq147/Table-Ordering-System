using System;

namespace WaiterApp.Services;

public sealed class ToastService
{
    // level: "info" | "error" | "success" ...
    public event Action<string, string>? OnShow;

    public void ShowError(string message) => Show(message, "error");
    public void ShowInfo(string message)  => Show(message, "info");
    public void ShowSuccess(string message) => Show(message, "success");

    private void Show(string message, string level)
    {
        if (string.IsNullOrWhiteSpace(message)) return;
        OnShow?.Invoke(message, level);
    }
}