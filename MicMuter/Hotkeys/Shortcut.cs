using System.Text;
using Avalonia.Input;

namespace MicMuter.Hotkeys;

public readonly record struct Shortcut(Key Key, KeyModifiers Modifiers)
{
    public static implicit operator (Key, KeyModifiers)(Shortcut shortcut) => (shortcut.Key, shortcut.Modifiers);
    public static explicit operator Shortcut ((Key, KeyModifiers) tuple) => new(tuple.Item1, tuple.Item2);

    public override string ToString()
    {
        StringBuilder sb = new();
        
        if (Modifiers.HasFlag(KeyModifiers.Control)) sb.Append("Ctrl + ");
        if (Modifiers.HasFlag(KeyModifiers.Shift)) sb.Append("Shift + ");
        if (Modifiers.HasFlag(KeyModifiers.Alt)) sb.Append("Alt + ");
        if (Modifiers.HasFlag(KeyModifiers.Meta)) sb.Append("Win + ");
        
        if (Key != Key.None) sb.Append($"{Key}");
        
        return sb.ToString();
    }
}
