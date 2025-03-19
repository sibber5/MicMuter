using System;
using System.Diagnostics;
using NAudio.CoreAudioApi;

namespace MicMuter.Audio;

internal sealed class WasapiMicDevice(MMDevice device) : IMicDevice
{
    public string Id => device.ID;
    
    public string FriendlyName => device.FriendlyName;
    
    public bool IsMuted => device.AudioEndpointVolume.Mute;
    
    private event EventHandler<bool>? _muteStatusChanged;
    public event EventHandler<bool> MuteStatusChanged
    {
        add
        {
            if (_muteStatusChanged is null)
            {
                Debug.WriteLine($"\n[{nameof(WasapiMicDevice)}] REGISTERING VOLUME NOTIFICATION FOR MIC {device.FriendlyName}\n");
                device.AudioEndpointVolume.OnVolumeNotification += OnVolumeNotification;
            }
            _muteStatusChanged += value;
        }
        remove
        {
            _muteStatusChanged -= value;
            if (_muteStatusChanged is null)
            {
                Debug.WriteLine($"\n[{nameof(WasapiMicDevice)}] UNREGISTERING VOLUME NOTIFICATION FOR MIC {device.FriendlyName}\n");
                device.AudioEndpointVolume.OnVolumeNotification -= OnVolumeNotification;
                _prevMuted = null;
            }
        }
    }

    private bool? _prevMuted = null;
    private void OnVolumeNotification(AudioVolumeNotificationData data)
    {
        if (_prevMuted is null || _prevMuted != data.Muted) _muteStatusChanged?.Invoke(this, data.Muted);
        _prevMuted = data.Muted;
    }

    public void ToggleMute()
    {
        device.AudioEndpointVolume.Mute = !device.AudioEndpointVolume.Mute;
    }
}
