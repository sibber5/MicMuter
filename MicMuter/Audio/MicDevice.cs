using System;

namespace MicMuter.Audio;

public class MicDevice(string id, string friendlyName, Action<object?> toggleMute, Func<object?, bool> isMuted, object? state = null)
{
    public string Id => id;

    public string FriendlyName => friendlyName;

    public bool IsMuted => isMuted(state);

    public void ToggleMute() => toggleMute(state);
}