using CommunityToolkit.Mvvm.ComponentModel;
using MicMuter.Audio;
using MicMuter.Hotkeys;

namespace MicMuter;

public sealed partial class Settings : ObservableObject
{
    [ObservableProperty]
    private IMicDevice? _micDevice;
    
    [ObservableProperty]
    private Shortcut _muteShortcut;
}
