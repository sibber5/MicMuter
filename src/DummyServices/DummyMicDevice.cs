#if DEBUG
using System;
using MicMuter.Audio;

namespace MicMuter.DummyServices;

internal sealed class DummyMicDevice : IMicDevice
{
    public string Id { get; } = "1";
    public string FriendlyName { get; } = "Dummy Mic";
    public bool IsMuted { get; private set; } = true;
    public void ToggleMute()
    {
        IsMuted = !IsMuted;
        MuteStatusChanged?.Invoke(this, IsMuted);
    }

    public event EventHandler<bool>? MuteStatusChanged;
}
#endif
