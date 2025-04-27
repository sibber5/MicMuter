using System;
using Microsoft.Extensions.Logging;
using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;
using static MicMuter.Audio.Windows.WindowsMicDeviceManager;

namespace MicMuter.Audio.Windows;

internal sealed class WasapiDefaultMicDeviceRef : IMicDevice
{
    private static readonly Lazy<WasapiDefaultMicDeviceRef> _instance = new(() => Avalonia.Threading.Dispatcher.UIThread.Invoke(() => new WasapiDefaultMicDeviceRef()));
    public static IMicDevice Instance { get; } = _instance.Value;

    public string Id => "467fc274-0b3f-4245-99c5-6a3dacc01d7f";
    public string FriendlyName => $"Default ({Device.FriendlyName})";
    public bool IsMuted => Device.AudioEndpointVolume.Mute;
    public void ToggleMute() => Device.AudioEndpointVolume.Mute = !Device.AudioEndpointVolume.Mute;

    public event EventHandler<bool>? MuteStatusChanged;
    
    private readonly ILogger<WasapiDefaultMicDeviceRef> _logger = StaticLogger.CreateLogger<WasapiDefaultMicDeviceRef>();
    
    private MMDevice? _device;
    private MMDevice Device
    {
        get => _device!;
        set
        {
            if (_device == value) return;

            if (_device is not null)
            {
                _device.AudioEndpointVolume.OnVolumeNotification -= OnVolumeNotification;
                _logger.LogInformation("Unregistered volume notification for {DeviceFriendlyName}", _device.FriendlyName);
            }
            
            value.AudioEndpointVolume.OnVolumeNotification += OnVolumeNotification;
            _logger.LogInformation("Registered volume notification for {DeviceFriendlyName}", value.FriendlyName);
            OnMuteStatusChanged(value.AudioEndpointVolume.Mute);
            
            _device = value;
        }
    }

    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    // we need to keep a reference to the com object
    private readonly MMNotificationClientImpl _notificationClient;
    
    private WasapiDefaultMicDeviceRef()
    {
        Device = DeviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Communications);
        _notificationClient = new(this);
        DeviceEnumerator.RegisterEndpointNotificationCallback(_notificationClient);
    }

    private bool? _prevMuted = null;
    private void OnVolumeNotification(AudioVolumeNotificationData data)
    {
        if (_prevMuted is null || _prevMuted != data.Muted) OnMuteStatusChanged(data.Muted);
        _prevMuted = data.Muted;
    }

    private void OnMuteStatusChanged(bool isMuted) => MuteStatusChanged?.Invoke(this, isMuted);

    private sealed class MMNotificationClientImpl(WasapiDefaultMicDeviceRef defaultDeviceRef) : IMMNotificationClient
    {
        public void OnDefaultDeviceChanged(DataFlow flow, Role role, string defaultDeviceId)
        {
            if (flow == DataFlow.Capture && role == Role.Communications)
            {
                defaultDeviceRef.Device = DeviceEnumerator.GetDevice(defaultDeviceId);
            }
        }
        
        public void OnDeviceStateChanged(string deviceId, DeviceState newState) { }
        public void OnDeviceAdded(string pwstrDeviceId) { }
        public void OnDeviceRemoved(string deviceId) { }
        public void OnPropertyValueChanged(string pwstrDeviceId, PropertyKey key) { }
    }
}
