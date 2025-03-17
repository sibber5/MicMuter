using System;
using Avalonia.Input;

namespace MicMuter.Hotkeys;

internal static class InputHelpers
{
    public static uint KeyToWindowsVirtualKey(Key key) => key switch
    {
        _ => throw new NotSupportedException()
    };
}