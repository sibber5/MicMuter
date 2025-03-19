using System;
using System.ComponentModel;
using MicMuter.Audio;
using MicMuter.Hotkeys;

namespace MicMuter;

internal sealed class MicMuterService : IDisposable
{
    public bool IsMicMuted { get; private set; }
    
    public event EventHandler<bool>? MuteStatusChanged;
    
    private IGlobalHotkey? _hotkey;
    private readonly IGlobalHotkeyFactory _hotkeyFactory;

    private MicDevice? _mic;
    
    public MicMuterService(IGlobalHotkeyFactory hotkeyFactory, Settings settings)
    {
        _hotkeyFactory = hotkeyFactory;
        _mic = settings.MicDevice;
        UpdateIsMicMuted();
        UpdateMuteShortcut(settings.MuteShortcut);
        settings.PropertyChanged += Settings_OnPropertyChanged;
    }

    private void Settings_OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        var settings = (Settings)sender!;
        if (nameof(Settings.MuteShortcut).Equals(e.PropertyName, StringComparison.Ordinal))
        {
            UpdateMuteShortcut(settings.MuteShortcut);
        }
        else if (nameof(Settings.MicDevice).Equals(e.PropertyName, StringComparison.Ordinal))
        {
            _mic = settings.MicDevice;
            UpdateIsMicMuted();
        }
    }

    private void OnHotkeyPressed(object? sender, EventArgs e)
    {
        if (_mic is null) return;
        
        _mic.ToggleMute();
        UpdateIsMicMuted();
    }

    private void UpdateMuteShortcut(Shortcut newShortcut)
    {
        if (_hotkey is not null && _hotkey.Shortcut == newShortcut) return;
        
        _hotkey?.Dispose();

        if (newShortcut == default)
        {
            _hotkey = null;
            return;
        }

        _hotkey = _hotkeyFactory.Register(newShortcut, 0);
        _hotkey.Pressed += OnHotkeyPressed;
    }

    private void UpdateIsMicMuted()
    {
        if (_mic is null) return;
        IsMicMuted = _mic.IsMuted;
        MuteStatusChanged?.Invoke(this, IsMicMuted);
    }
    
    public void Dispose() => _hotkey?.Dispose();
}
