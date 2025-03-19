namespace MicMuter.Hotkeys;

internal interface IGlobalHotkeyFactory
{
    IGlobalHotkey Register(Shortcut shortcut, nint hWnd);
}
