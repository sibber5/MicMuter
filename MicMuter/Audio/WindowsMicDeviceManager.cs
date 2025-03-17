using System.Collections.Generic;

namespace MicMuter.Audio;

public class WindowsMicDeviceManager : IMicDeviceManager
{
    public IReadOnlyList<MicDevice> GetMicDevices()
    {
        return
        [
            new("", "Mic 1", null!, null!),
            new("", "Mic 2", null!, null!),
            new("", "AAAAAAAAAAMic 3", null!, null!),
            new("", "Mic 4", null!, null!)
        ];
    }
}