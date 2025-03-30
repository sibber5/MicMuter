using System;
using System.ComponentModel;
using Avalonia.Platform;
using MicMuter.AppSettings;
using MicMuter.Audio;
using MicMuter.Hotkeys;

namespace MicMuter.MiscServices;

internal sealed class MicMuterService : IDisposable
{
    public bool IsMicMuted => Mic?.IsMuted == true;
    
    public event EventHandler<bool>? MuteStatusChanged;
    
    private IGlobalHotkey? _hotkey;
    private readonly IGlobalHotkeyFactory _hotkeyFactory;

    private readonly LazyService<Func<IPlatformHandle?>> _getMainWindowHandle;

    private IMicDevice? _mic;
    private IMicDevice? Mic
    {
        get => _mic;
        set
        {
            if (_mic == value) return;

            if (_mic is null)
            {
                _mic = value!;
                _mic.MuteStatusChanged += MicOnMuteStatusChanged;
                MuteStatusChanged?.Invoke(this, _mic.IsMuted);
                return;
            }
            
            _mic.MuteStatusChanged -= MicOnMuteStatusChanged;
            _mic = value;
            if (_mic is not null) _mic.MuteStatusChanged += MicOnMuteStatusChanged;
            MuteStatusChanged?.Invoke(this, _mic?.IsMuted == true);
        }
    }

    public MicMuterService(IGlobalHotkeyFactory hotkeyFactory, Settings settings, LazyService<Func<IPlatformHandle?>> getMainWindowHandle)
    {
        _hotkeyFactory = hotkeyFactory;
        _getMainWindowHandle = getMainWindowHandle;
        Mic = settings.MicDevice;
        UpdateMuteShortcut(settings.MuteShortcut);
        settings.PropertyChanged += Settings_OnPropertyChanged;
    }

    private void MicOnMuteStatusChanged(object? sender, bool e) => MuteStatusChanged?.Invoke(this, e);

    private void Settings_OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        var settings = (Settings)sender!;
        if (nameof(Settings.MuteShortcut).Equals(e.PropertyName, StringComparison.Ordinal))
        {
            UpdateMuteShortcut(settings.MuteShortcut);
        }
        else if (nameof(Settings.MicDevice).Equals(e.PropertyName, StringComparison.Ordinal))
        {
            Mic = settings.MicDevice;
        }
    }

    private void OnHotkeyPressed(object? sender, EventArgs e)
    {
        Mic?.ToggleMute();
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

        _hotkey = _hotkeyFactory.Register(newShortcut, _getMainWindowHandle.Value()!.Handle);
        _hotkey.Pressed += OnHotkeyPressed;
        Helpers.DebugWriteLine($"Registered new hotkey: {_hotkey.Shortcut}");
    }

    public void Dispose() => _hotkey?.Dispose();
}
