using System;
using System.Runtime.InteropServices;

namespace MicMuter.Hotkeys;

internal sealed class WindowsGlobalHotkey : IGlobalHotkey
{
    public event EventHandler? Pressed;

    public Shortcut Shortcut { get; }

    private readonly nint _hWnd;
    private readonly int _id;
    private readonly WindowMessageMonitor _monitor;

    private bool _disposed = false;

    internal WindowsGlobalHotkey(Shortcut shortcut, nint hWnd, int id)
    {
        Shortcut = shortcut;
        _hWnd = hWnd;
        _id = id;
        _monitor = new(_hWnd);
        _monitor.WindowMessageReceived += (_, _) => Pressed?.Invoke(this, EventArgs.Empty);
    }

    private void Unregister()
    {
        if (_disposed) return;
        _monitor.Dispose();
        PInvoke.UnregisterHotKey(_hWnd, _id);
        _disposed = true;
    }

    public void Dispose()
    {
        Unregister();
        GC.SuppressFinalize(this);
    }

    ~WindowsGlobalHotkey()
    {
        Unregister();
    }
}

internal static partial class PInvoke
{
    [LibraryImport("User32", SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool UnregisterHotKey(nint hWnd, int id);
}
