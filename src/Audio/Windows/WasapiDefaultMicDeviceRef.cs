using System;
using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;
using static MicMuter.Audio.Windows.WindowsMicDeviceManager;

namespace MicMuter.Audio.Windows;

internal sealed class WasapiDefaultMicDeviceRef : IMicDevice
{
    // private static readonly Lazy<WasapiDefaultMicDeviceRef> _instance = new(() => new WasapiDefaultMicDeviceRef());
    public static IMicDevice Instance { get; } = new WasapiDefaultMicDeviceRef();

    public string Id => "467fc274-0b3f-4245-99c5-6a3dacc01d7f";
    public string FriendlyName => $"Default ({Device.FriendlyName})";
    public bool IsMuted => Device.AudioEndpointVolume.Mute;
    public void ToggleMute() => Device.AudioEndpointVolume.Mute = !Device.AudioEndpointVolume.Mute;

    public event EventHandler<bool>? MuteStatusChanged;

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
                Helpers.DebugWriteLine($"Unregistered volume notification for {_device.FriendlyName}");
            }
            
            value.AudioEndpointVolume.OnVolumeNotification += OnVolumeNotification;
            Helpers.DebugWriteLine($"Registered volume notification for {value.FriendlyName}");
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
