#if DEBUG
using System;
using MicMuter.Hotkeys;

namespace MicMuter.DummyServices;

internal sealed class DummyGlobalHotkeyFactory : IGlobalHotkeyFactory
{
    public IGlobalHotkey Register(Shortcut shortcut, bool ignoreExtraModifiers, IntPtr hWnd) => new DummyGlobalHotkey();
}
#endif
