using System.Collections.Generic;

namespace MicMuter.Audio;

public interface IMicDeviceManager
{
    IReadOnlyList<IMicDevice> GetMicDevices();
    IMicDevice GetDefaultMicDevice();
    IMicDevice GetMicDeviceById(string id);
}