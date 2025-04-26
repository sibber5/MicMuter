#if DEBUG
using System;
using MicMuter.Hotkeys;

namespace MicMuter.DummyServices;

internal sealed class DummyGlobalHotkey : IGlobalHotkey
{
    public void Dispose() {}

    public event EventHandler? Pressed;
    public Shortcut Shortcut { get; } = default;
    public bool IgnoresExtraModifiers { get; } = default;
}
#endif
