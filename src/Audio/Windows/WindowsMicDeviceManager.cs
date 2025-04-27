using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using NAudio.CoreAudioApi;

namespace MicMuter.Audio.Windows;

public class WindowsMicDeviceManager : IMicDeviceManager
{
    internal static readonly MMDeviceEnumerator DeviceEnumerator = new();
    
    private static readonly ConcurrentDictionary<string, IMicDevice> _deviceMap = new();
    private static IMicDevice ToMicDevice(MMDevice mmDevice)
        => WasapiDefaultMicDeviceRef.Instance.Id.Equals(mmDevice.ID, StringComparison.Ordinal)
            ? WasapiDefaultMicDeviceRef.Instance
            : _deviceMap.GetOrAdd(mmDevice.ID, static (_, d) => new WasapiMicDevice(d, StaticLogger.CreateLogger<WasapiMicDevice>()), mmDevice);
    
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
            : _deviceMap.GetOrAdd(id, static id => new WasapiMicDevice(DeviceEnumerator.GetDevice(id), StaticLogger.CreateLogger<WasapiMicDevice>()));
}
