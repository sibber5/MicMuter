using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;
using Avalonia.Input;

namespace MicMuter.Hotkeys;

internal sealed class WindowsGlobalHotkeyFactory : IGlobalHotkeyFactory
{
    /// <remarks>This method is thread-safe.</remarks>
    public IGlobalHotkey Register(Shortcut shortcut, nint hWnd) => WindowsHotkeyFactory.Register(shortcut, hWnd);
    
    private static class WindowsHotkeyFactory
    {
        private static int _lastId = 0;
        private static readonly ConcurrentDictionary<(Shortcut, nint), WindowsGlobalHotkey> _registeredHotkeys = new();

        /// <remarks>This method is thread-safe.</remarks>
        public static IGlobalHotkey Register(Shortcut shortcut, nint hWnd)
        {
            if (Interlocked.Increment(ref _lastId) >= 0xBFFF)
                throw new InvalidOperationException("Max number of hotkeys exceeded.");

            return _registeredHotkeys.GetOrAdd((shortcut, hWnd), static (key, id) =>
            {
                var (shortcut, hWnd) = key;

                uint modifiers = 0x4000;
                if (shortcut.Modifiers.HasFlag(KeyModifiers.Alt)) modifiers |= 0x0001;
                if (shortcut.Modifiers.HasFlag(KeyModifiers.Control)) modifiers |= 0x0002;
                if (shortcut.Modifiers.HasFlag(KeyModifiers.Shift)) modifiers |= 0x0004;
                if (shortcut.Modifiers.HasFlag(KeyModifiers.Meta)) modifiers |= 0x0008;

                uint vk = InputHelpers.KeyToWindowsVirtualKey(shortcut.Key);

                if (!PInvoke.RegisterHotKey(hWnd, id, modifiers, vk)) throw new Win32Exception();

                return new WindowsGlobalHotkey(shortcut, hWnd, id);
            }, _lastId);
        }
    }
}

internal static partial class PInvoke
{
    [LibraryImport("User32", SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool RegisterHotKey(nint hWnd, int id, uint fsModifiers, uint vk);
}
