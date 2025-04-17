using System;
using System.Runtime.InteropServices;
using MicMuter.NativeMisc.Windows;

namespace MicMuter.Hotkeys.Windows;

internal sealed class WindowsGlobalHotkey : IGlobalHotkey
{
    public event EventHandler? Pressed;

    public Shortcut Shortcut { get; }

    public bool IgnoresExtraModifiers => false;

    private readonly nint _hWnd;
    private readonly int _id;
    private readonly WindowMessageMonitor _monitor;

    public bool Disposed { get; private set; } = false;

    internal WindowsGlobalHotkey(Shortcut shortcut, nint hWnd, int id)
    {
        Shortcut = shortcut;
        
        _hWnd = hWnd;
        _id = id;
        _monitor = new(_hWnd);
        _monitor.WindowMessageReceived += (_, e) =>
        {
            //                         WM_HOTKEY
            if (e.Message.MessageId == 0x0312 && unchecked((int)e.Message.WParam) == _id) Pressed?.Invoke(this, EventArgs.Empty);
        };
        
        Helpers.DebugWriteLine($"Created hotkey {_id}: {Shortcut}");
    }

    private void Unregister()
    {
        if (Disposed) return;
        Helpers.DebugWriteLine($"Disposed hotkey {_id}: {Shortcut}");
        _monitor.Dispose();
        if (!PInvoke.UnregisterHotKey(_hWnd, _id)) Helpers.DebugWriteLine($"Failed to unregister hotkey {Shortcut} with id {_id}. Error: {Marshal.GetLastPInvokeError()}");
        Disposed = true;
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
