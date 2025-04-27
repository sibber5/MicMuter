using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using Avalonia.Input;

namespace MicMuter.Hotkeys.Windows;

internal sealed class WindowsGlobalHotkeyFactory : IGlobalHotkeyFactory
{
    public IGlobalHotkey Register(Shortcut shortcut, bool ignoreExtraModifiers, nint hWnd)
    {
        if (!ignoreExtraModifiers) return WindowsHotkeyFactoryImpl.Register(shortcut, hWnd);

        if (shortcut.Modifiers.HasFlag(KeyModifiers.Meta)) throw new NotSupportedException();
        
        if (shortcut.Modifiers == (KeyModifiers.Alt | KeyModifiers.Control | KeyModifiers.Shift)) return WindowsHotkeyFactoryImpl.Register(shortcut, hWnd);
        
        Debug.Assert(Enum.GetValues<KeyModifiers>() is [0, (KeyModifiers)1, (KeyModifiers)2, (KeyModifiers)4, (KeyModifiers)8]);
        
        IGlobalHotkey[] hotkeys;
        if (shortcut.Modifiers == KeyModifiers.None)
        {
            const int optionCount = 8; // 2^3
            hotkeys = new IGlobalHotkey[optionCount];
            // register every possible modifier key combination
            for (int i = 0; i < optionCount; i++)
            {
                hotkeys[i] = WindowsHotkeyFactoryImpl.Register(shortcut with { Modifiers = (KeyModifiers)i }, hWnd);
            }
        }
        else
        {
            // register every possible modifier key combination, except for the ones which dont include the modifier key from the shortcut
            hotkeys = shortcut.Modifiers switch
            {
                KeyModifiers.Alt =>
                [
                    WindowsHotkeyFactoryImpl.Register(shortcut, hWnd),
                    WindowsHotkeyFactoryImpl.Register(shortcut with { Modifiers = KeyModifiers.Alt | KeyModifiers.Control }, hWnd),
                    WindowsHotkeyFactoryImpl.Register(shortcut with { Modifiers = KeyModifiers.Alt | KeyModifiers.Shift }, hWnd),
                    WindowsHotkeyFactoryImpl.Register(shortcut with { Modifiers = KeyModifiers.Alt | KeyModifiers.Control | KeyModifiers.Shift }, hWnd),
                ],
                KeyModifiers.Control =>
                [
                    WindowsHotkeyFactoryImpl.Register(shortcut, hWnd),
                    WindowsHotkeyFactoryImpl.Register(shortcut with { Modifiers = KeyModifiers.Control | KeyModifiers.Alt }, hWnd),
                    WindowsHotkeyFactoryImpl.Register(shortcut with { Modifiers = KeyModifiers.Control | KeyModifiers.Shift }, hWnd),
                    WindowsHotkeyFactoryImpl.Register(shortcut with { Modifiers = KeyModifiers.Alt | KeyModifiers.Control | KeyModifiers.Shift }, hWnd),
                ],
                KeyModifiers.Shift =>
                [
                    WindowsHotkeyFactoryImpl.Register(shortcut, hWnd),
                    WindowsHotkeyFactoryImpl.Register(shortcut with { Modifiers = KeyModifiers.Shift | KeyModifiers.Alt }, hWnd),
                    WindowsHotkeyFactoryImpl.Register(shortcut with { Modifiers = KeyModifiers.Shift | KeyModifiers.Control }, hWnd),
                    WindowsHotkeyFactoryImpl.Register(shortcut with { Modifiers = KeyModifiers.Alt | KeyModifiers.Control | KeyModifiers.Shift }, hWnd),
                ],
                _ =>
                [
                    WindowsHotkeyFactoryImpl.Register(shortcut, hWnd),
                    WindowsHotkeyFactoryImpl.Register(shortcut with { Modifiers = KeyModifiers.Alt | KeyModifiers.Control | KeyModifiers.Shift }, hWnd),
                ]
            };   
        }
        
        return new CompositeGlobalHotkey(shortcut, true, hotkeys);
    }
}

file static class WindowsHotkeyFactoryImpl
{
    private static int _lastId = 0;
    private static readonly ConcurrentDictionary<(Shortcut, nint), WindowsGlobalHotkey> _registeredHotkeys = new();
    private static readonly Lock _lock = new();

    /// <remarks>This method is thread-safe.</remarks>
    public static WindowsGlobalHotkey Register(Shortcut shortcut, nint hWnd)
    {
        if (Interlocked.Increment(ref _lastId) >= 0xBFFF) throw new InvalidOperationException("Max number of hotkeys exceeded.");

        WindowsGlobalHotkey? hotkey = null;

        if (_registeredHotkeys.TryGetValue((shortcut, hWnd), out hotkey))
        {
            if (hotkey.Disposed)
            {
                lock (_lock)
                {
                    if (hotkey.Disposed)
                    {
                        hotkey = CreateValue(shortcut, hWnd, _lastId);
                    }
                }
                
                _registeredHotkeys[(shortcut, hWnd)] = hotkey;
            }
            
            return hotkey;
        }

        if (hotkey is null)
        {
            lock (_lock)
            {
                hotkey ??= CreateValue(shortcut, hWnd, _lastId);
            }
        }
        
        _registeredHotkeys[(shortcut, hWnd)] = hotkey;

        return hotkey;

        static WindowsGlobalHotkey CreateValue(Shortcut shortcut, nint hWnd, int id)
        {
            uint modifiers = 0x4000;
            if (shortcut.Modifiers.HasFlag(KeyModifiers.Alt)) modifiers |= 0x0001;
            if (shortcut.Modifiers.HasFlag(KeyModifiers.Control)) modifiers |= 0x0002;
            if (shortcut.Modifiers.HasFlag(KeyModifiers.Shift)) modifiers |= 0x0004;
            if (shortcut.Modifiers.HasFlag(KeyModifiers.Meta)) modifiers |= 0x0008;

            uint vk = InputHelpers.KeyToWindowsVirtualKey(shortcut.Key);

            if (!PInvoke.RegisterHotKey(hWnd, id, modifiers, vk)) throw new Win32Exception();

            return new WindowsGlobalHotkey(shortcut, hWnd, id, StaticLogger.CreateLogger<WindowsGlobalHotkey>());
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
