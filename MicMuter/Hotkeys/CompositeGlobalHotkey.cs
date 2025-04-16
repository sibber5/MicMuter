using System;
using System.Collections.Generic;

namespace MicMuter.Hotkeys;

public sealed class CompositeGlobalHotkey : IGlobalHotkey
{
    public event EventHandler? Pressed;
    
    public Shortcut Shortcut { get; }

    public bool IgnoresExtraModifiers { get; }
    
    private readonly IEnumerable<IGlobalHotkey> _hotkeys;

    public CompositeGlobalHotkey(Shortcut shortcut, bool ignoresExtraModifiers, IEnumerable<IGlobalHotkey> hotkeys)
    {
        Shortcut = shortcut;
        IgnoresExtraModifiers = ignoresExtraModifiers;
        
        _hotkeys = hotkeys;
        foreach (var hotkey in _hotkeys)
        {
            hotkey.Pressed += OnAnyHotkeyPressed;
        }
    }

    private void OnAnyHotkeyPressed(object? sender, EventArgs e)
    {
        Pressed?.Invoke(this, e);
    }

    public void Dispose()
    {
        foreach (var hotkey in _hotkeys)
        {
            hotkey.Pressed -= OnAnyHotkeyPressed;
            hotkey.Dispose();
        }
    }
}
