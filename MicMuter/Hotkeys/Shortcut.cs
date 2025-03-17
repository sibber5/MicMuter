using Avalonia.Input;

namespace MicMuter.Hotkeys;

public readonly record struct Shortcut(Key Key, KeyModifiers Modifiers)
{
    public static implicit operator (Key, KeyModifiers)(Shortcut shortcut) => (shortcut.Key, shortcut.Modifiers);
    public static explicit operator Shortcut ((Key, KeyModifiers) tuple) => new(tuple.Item1, tuple.Item2);
}
