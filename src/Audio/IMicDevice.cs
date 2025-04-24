using System;

namespace MicMuter.Audio;

public interface IMicDevice
{
    string Id { get; }
    string FriendlyName { get; }
    bool IsMuted { get; }
    void ToggleMute();
    event EventHandler<bool> MuteStatusChanged;
}
