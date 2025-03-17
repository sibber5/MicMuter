using System.Collections.Generic;

namespace MicMuter.Audio;

public interface IMicDeviceManager
{
    IReadOnlyList<MicDevice> GetMicDevices();
}