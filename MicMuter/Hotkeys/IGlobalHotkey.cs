using System;

namespace MicMuter.Hotkeys;

public interface IGlobalHotkey : IDisposable
{
    event EventHandler Pressed;
    
    Shortcut Shortcut { get; }
}