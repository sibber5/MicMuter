using Avalonia.Controls;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MicMuter;

internal sealed partial class TrayIconViewModel : ObservableObject
{
    private readonly WindowIcon _unmutedIcon = new(AssetLoader.Open(new("avares://MicMuter/Assets/icon_unmuted.ico")));
    private readonly WindowIcon _mutedIcon = new(AssetLoader.Open(new("avares://MicMuter/Assets/icon_muted.ico")));
    
    [ObservableProperty]
    private WindowIcon _currentIcon;

    public TrayIconViewModel(MicMuterService micMuterService)
    {
        _currentIcon = _unmutedIcon;
        micMuterService.MuteStatusChanged += MicMuterService_OnMuteStatusChanged;
    }

    private void MicMuterService_OnMuteStatusChanged(object? sender, bool isMuted)
    {
        CurrentIcon = isMuted ? _mutedIcon : _unmutedIcon;
    }
}
