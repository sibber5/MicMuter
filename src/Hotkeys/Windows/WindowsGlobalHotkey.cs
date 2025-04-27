using System;
using System.Runtime.InteropServices;
using MicMuter.NativeMisc.Windows;
using Microsoft.Extensions.Logging;

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
    
    private readonly ILogger<WindowsGlobalHotkey> _logger;
    
    internal WindowsGlobalHotkey(Shortcut shortcut, nint hWnd, int id, ILogger<WindowsGlobalHotkey> logger)
    {
        _logger = logger;
        
        Shortcut = shortcut;
        
        _hWnd = hWnd;
        _id = id;
        _monitor = new(_hWnd);
        _monitor.WindowMessageReceived += (_, e) =>
        {
            //                         WM_HOTKEY
            if (e.Message.MessageId == 0x0312 && unchecked((int)e.Message.WParam) == _id)
            {
                _logger.LogDebug("Triggered {Shortcut} with id {Id}", Shortcut, _id);
                Pressed?.Invoke(this, EventArgs.Empty);
            }
        };
        
        _logger.LogInformation("Created hotkey {Id}: {Shortcut}", _id, Shortcut);
    }

    private void Unregister()
    {
        if (Disposed) return;
        _logger.LogInformation("Disposed hotkey {Id}: {Shortcut}", _id, Shortcut);
        _monitor.Dispose();
        if (!PInvoke.UnregisterHotKey(_hWnd, _id)) _logger.LogError("Failed to unregister hotkey {Shortcut} with id {Id}. Error: {PInvokeError}", Shortcut, _id, Marshal.GetLastPInvokeError());
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
