using System.Collections.Generic;

namespace MicMuter.Audio;

public interface IMicDeviceManager
{
    IReadOnlyList<MicDevice> GetMicDevices();
    MicDevice GetDefaultMicDevice();
    MicDevice GetMicDeviceById(string id);
}