using System;

namespace MicMuter.Hotkeys;

public interface IGlobalHotkey : IDisposable
{
    static abstract IGlobalHotkey Register(Shortcut shortcut, nint hWnd);

    event EventHandler Pressed;
    
    Shortcut Shortcut { get; }
}