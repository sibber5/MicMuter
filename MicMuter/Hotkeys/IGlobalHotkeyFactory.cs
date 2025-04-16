namespace MicMuter.Hotkeys;

public interface IGlobalHotkeyFactory
{
    /// <param name="shortcut">The key + modifiers combination.</param>
    /// <param name="ignoreExtraModifiers">
    /// If <see langword="false"/> then the state of <i>all</i> modifier keys must match for the hotkey to be triggered
    /// (e.g. for the hotkey Ctrl + F7, it will <i>not</i> be triggered if Shift or Alt are held down).<br/>
    /// Otherwise only the state of the modifier key specified in the shortcut must match. 
    /// </param>
    /// <param name="hWnd">The handle of the window to which to register the hotkey.</param>
    IGlobalHotkey Register(Shortcut shortcut, bool ignoreExtraModifiers, nint hWnd);
}
