using Microsoft.AspNetCore.Components;
using Microsoft.Maui.ApplicationModel;

namespace WaiterApp.Services;

/// <summary>
/// Bridges MAUI layer events (alerts, configuration issues) to the Blazor NavigationManager.
/// </summary>
public sealed class NavigationBridge
{
    private readonly object _gate = new();
    private NavigationManager? _navigationManager;

    public void Register(NavigationManager navigationManager)
    {
        if (navigationManager == null) throw new ArgumentNullException(nameof(navigationManager));
        lock (_gate)
        {
            _navigationManager ??= navigationManager;
        }
    }

    public bool TryNavigate(string uri, bool forceLoad = false)
    {
        if (string.IsNullOrWhiteSpace(uri)) return false;

        NavigationManager? navigationManager;
        lock (_gate)
        {
            navigationManager = _navigationManager;
        }

        if (navigationManager is null) return false;

        void Navigate() => navigationManager.NavigateTo(uri, forceLoad);

        if (MainThread.IsMainThread)
        {
            Navigate();
        }
        else
        {
            MainThread.BeginInvokeOnMainThread(Navigate);
        }

        return true;
    }
}
