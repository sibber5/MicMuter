using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using NAudio.CoreAudioApi;

namespace MicMuter.Audio;

public class WindowsMicDeviceManager : IMicDeviceManager
{
    private static readonly ConcurrentDictionary<string, IMicDevice> _deviceMap = new();
    internal static IMicDevice ToMicDevice(MMDevice mmDevice)
        // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
        // WasapiDefaultMicDeviceRef.Instance could be null in this case because both this method and WasapiDefaultMicDeviceRef.Instance are static,
        // so it could be, and indeed is (in the WasapiDefaultMicDeviceRef ctor), called before the static instance is instantiated.
        => WasapiDefaultMicDeviceRef.Instance?.Id.Equals(mmDevice.ID, StringComparison.Ordinal) == true
            ? WasapiDefaultMicDeviceRef.Instance
            : _deviceMap.GetOrAdd(mmDevice.ID, static (_, d) => new WasapiMicDevice(d), mmDevice);
    
    internal static readonly MMDeviceEnumerator DeviceEnumerator = new();

    private MMDeviceCollection? _lastDeviceCol;
    private IReadOnlyList<IMicDevice>? _lastIMicDeviceList;
    public IReadOnlyList<IMicDevice> GetMicDevices()
    {
        var deviceCol = DeviceEnumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);
        if (_lastDeviceCol is null || !_lastDeviceCol.Select(x => x.ID).SequenceEqual(deviceCol.Select(x => x.ID)))
        {
            _lastDeviceCol = deviceCol;
            _lastIMicDeviceList = [WasapiDefaultMicDeviceRef.Instance, ..deviceCol.Select(ToMicDevice)];
        }
        
        return _lastIMicDeviceList!;
    }

    public IMicDevice GetDefaultMicDevice() => WasapiDefaultMicDeviceRef.Instance;

    public IMicDevice GetMicDeviceById(string id)
        => WasapiDefaultMicDeviceRef.Instance.Id.Equals(id, StringComparison.Ordinal)
            ? WasapiDefaultMicDeviceRef.Instance
            : ToMicDevice(DeviceEnumerator.GetDevice(id));
}
