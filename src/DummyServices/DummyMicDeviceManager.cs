#if DEBUG
using System.Collections.Generic;
using MicMuter.Audio;

namespace MicMuter.DummyServices;

internal sealed class DummyMicDeviceManager : IMicDeviceManager
{
    private readonly DummyMicDevice _dummyMicDevice = new();
    
    public IReadOnlyList<IMicDevice> GetMicDevices() => [_dummyMicDevice];

    public IMicDevice GetDefaultMicDevice() => _dummyMicDevice;

    public IMicDevice GetMicDeviceById(string id) => _dummyMicDevice;
}
#endif
