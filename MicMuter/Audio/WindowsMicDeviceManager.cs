using System.Collections.Generic;
using System.Linq;
using NAudio.CoreAudioApi;

namespace MicMuter.Audio;

public class WindowsMicDeviceManager : IMicDeviceManager
{
    private readonly MMDeviceEnumerator _enumerator = new();

    public IReadOnlyList<MicDevice> GetMicDevices()
        => _enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active)
            .Select(ToMicDevice)
            .ToList();

    public MicDevice GetDefaultMicDevice() => ToMicDevice(_enumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Communications));

    public MicDevice GetMicDeviceById(string id) => ToMicDevice(_enumerator.GetDevice(id));

    private static MicDevice ToMicDevice(MMDevice d) => new(
        d.ID,
        d.FriendlyName,
        (vol) =>
        {
            var volume = (AudioEndpointVolume)vol!;
            volume.Mute = !volume.Mute;
        },
        (vol) => ((AudioEndpointVolume)vol!).Mute,
        d.AudioEndpointVolume
    );
}