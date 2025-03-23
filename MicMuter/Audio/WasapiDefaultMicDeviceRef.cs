using System;
using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;
using static MicMuter.Audio.WindowsMicDeviceManager;

namespace MicMuter.Audio;

internal sealed class WasapiDefaultMicDeviceRef : IMicDevice
{
    // private static readonly Lazy<WasapiDefaultMicDeviceRef> _instance = new(() => new WasapiDefaultMicDeviceRef());
    public static IMicDevice Instance { get; } = new WasapiDefaultMicDeviceRef();

    public string Id => "467fc274-0b3f-4245-99c5-6a3dacc01d7f";
    public string FriendlyName => $"Default ({Device.FriendlyName})";
    public bool IsMuted => Device.IsMuted;
    public void ToggleMute() => Device.ToggleMute();

    public event EventHandler<bool>? MuteStatusChanged;

    private IMicDevice _device;
    private IMicDevice Device
    {
        get => _device;
        set
        {
            if (_device == value) return;

            // TODO: this causes some fucking com exception, figure out this fucking shit later i dont fucking know
            _device.MuteStatusChanged -= Device_OnMuteStatusChanged;
            value.MuteStatusChanged += Device_OnMuteStatusChanged;
            _device = value;
        }
    }
    
    private WasapiDefaultMicDeviceRef()
    {
        _device = ToMicDevice(DeviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Communications));
        _device.MuteStatusChanged += Device_OnMuteStatusChanged;
        DeviceEnumerator.RegisterEndpointNotificationCallback(new MMNotificationClientImpl(this));
    }

    private void Device_OnMuteStatusChanged(object? sender, bool e) => MuteStatusChanged?.Invoke(sender, e);

    private sealed class MMNotificationClientImpl(WasapiDefaultMicDeviceRef defaultDeviceRef) : IMMNotificationClient
    {
        public void OnDefaultDeviceChanged(DataFlow flow, Role role, string defaultDeviceId)
        {
            if (flow == DataFlow.Capture && role == Role.Communications)
            {
                defaultDeviceRef.Device = ToMicDevice(DeviceEnumerator.GetDevice(defaultDeviceId));
            }
        }
        
        public void OnDeviceStateChanged(string deviceId, DeviceState newState) { }
        public void OnDeviceAdded(string pwstrDeviceId) { }
        public void OnDeviceRemoved(string deviceId) { }
        public void OnPropertyValueChanged(string pwstrDeviceId, PropertyKey key) { }
    }
}
