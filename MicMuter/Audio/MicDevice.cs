using System;

namespace MicMuter.Audio;

public class MicDevice(string id, string friendlyName, Action<object?> toggleMute, Func<object?, bool> isMuted, object? data = null)
{
    public string Id => id;

    public string FriendlyName => friendlyName;

    public bool IsMuted => isMuted(data);

    public void ToggleMute() => toggleMute(data);
}