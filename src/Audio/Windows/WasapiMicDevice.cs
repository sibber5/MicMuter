using System;
using Microsoft.Extensions.Logging;
using NAudio.CoreAudioApi;

namespace MicMuter.Audio.Windows;

internal sealed class WasapiMicDevice(MMDevice device, ILogger<WasapiMicDevice> logger) : IMicDevice
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
                device.AudioEndpointVolume.OnVolumeNotification += OnVolumeNotification;
                logger.LogInformation("Registered volume notification for {DeviceFriendlyName}", device.FriendlyName);
            }
            _muteStatusChanged += value;
        }
        remove
        {
            _muteStatusChanged -= value;
            if (_muteStatusChanged is null)
            {
                device.AudioEndpointVolume.OnVolumeNotification -= OnVolumeNotification;
                logger.LogInformation("Unregistered volume notification for {DeviceFriendlyName}", device.FriendlyName);
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
