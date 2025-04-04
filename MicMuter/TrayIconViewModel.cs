using Avalonia.Controls;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using MicMuter.MiscServices;

namespace MicMuter;

internal sealed partial class TrayIconViewModel : ObservableObject
{
    private readonly WindowIcon _unmutedIcon = new(AssetLoader.Open(new("avares://MicMuter/Assets/icon_unmuted.ico")));
    private readonly WindowIcon _mutedIcon = new(AssetLoader.Open(new("avares://MicMuter/Assets/icon_muted.ico")));
    
    [ObservableProperty]
    private WindowIcon _currentIcon;

    private readonly MicMuterService _micMuterService;
    
    public TrayIconViewModel(MicMuterService micMuterService)
    {
        _micMuterService = micMuterService;
        _currentIcon = _micMuterService.IsMicMuted ? _mutedIcon : _unmutedIcon;
        _micMuterService.MuteStatusChanged += MicMuterService_OnMuteStatusChanged;
    }

    private void MicMuterService_OnMuteStatusChanged(object? sender, bool isMuted)
    {
        Helpers.DebugWriteLine($"Muted: {isMuted}");
        CurrentIcon = isMuted ? _mutedIcon : _unmutedIcon;
    }
}
