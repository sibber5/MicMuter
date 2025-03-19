using System.Collections.Generic;
using System.Linq;
using NAudio.CoreAudioApi;

namespace MicMuter.Audio;

public class WindowsMicDeviceManager : IMicDeviceManager
{
    private readonly Dictionary<string, WasapiMicDevice> _deviceMap = new();
    private readonly MMDeviceEnumerator _enumerator = new();

    private MMDeviceCollection? _lastDeviceCol;
    private IReadOnlyList<IMicDevice>? _lastIMicDeviceList;
    public IReadOnlyList<IMicDevice> GetMicDevices()
    {
        var deviceCol = _enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);
        if (_lastDeviceCol is null || !_lastDeviceCol.Select(x => x.ID).SequenceEqual(deviceCol.Select(x => x.ID)))
        {
            _lastDeviceCol = deviceCol;
            _lastIMicDeviceList = deviceCol.Select(ToMicDevice)
                .ToList();
        }
        
        return _lastIMicDeviceList!;
    }

    public IMicDevice GetDefaultMicDevice() => ToMicDevice(_enumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Communications));

    public IMicDevice GetMicDeviceById(string id) => ToMicDevice(_enumerator.GetDevice(id));

    private WasapiMicDevice ToMicDevice(MMDevice mmDevice)
    {
        WasapiMicDevice wasapiDevice;
        if (_deviceMap.TryGetValue(mmDevice.ID, out wasapiDevice!)) return wasapiDevice;

        wasapiDevice = new(mmDevice);
        _deviceMap[mmDevice.ID] = wasapiDevice;
        return wasapiDevice;
    }
}
