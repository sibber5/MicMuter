using CommunityToolkit.Mvvm.ComponentModel;
using MicMuter.Audio;
using MicMuter.Hotkeys;

namespace MicMuter.AppSettings;

public sealed partial class Settings : ObservableObject
{
    [ObservableProperty]
    private IMicDevice? _micDevice;
    
    [ObservableProperty]
    private Shortcut _muteShortcut;
    
    [ObservableProperty]
    private bool _runOnStartup;
    
    [ObservableProperty]
    private bool _startMinimized;
}
