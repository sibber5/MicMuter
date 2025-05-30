﻿using System;
using System.ComponentModel;
using Avalonia.Platform;
using MicMuter.AppSettings;
using MicMuter.Audio;
using MicMuter.Hotkeys;
using Microsoft.Extensions.Logging;

namespace MicMuter.MiscServices;

internal sealed class MicMuterService : IDisposable
{
    public bool IsMicMuted => Mic?.IsMuted == true;
    
    public event EventHandler<bool>? MuteStatusChanged;
    
    private IGlobalHotkey? _hotkey;
    private readonly IGlobalHotkeyFactory _hotkeyFactory;

    private readonly LazyService<Func<IPlatformHandle?>> _getMainWindowHandle;
    
    private readonly ILogger<MicMuterService> _logger;

    private IMicDevice? _mic;
    private IMicDevice? Mic
    {
        get => _mic;
        set
        {
            if (_mic == value) return;

            if (_mic is null)
            {
                ArgumentNullException.ThrowIfNull(value);
                _mic = value;
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

    public MicMuterService(IGlobalHotkeyFactory hotkeyFactory, Settings settings, LazyService<Func<IPlatformHandle?>> getMainWindowHandle, ILogger<MicMuterService> logger)
    {
        _hotkeyFactory = hotkeyFactory;
        _getMainWindowHandle = getMainWindowHandle;
        _logger = logger;
        Mic = settings.MicDevice;
        UpdateMuteShortcut(settings.MuteShortcut, settings.IgnoreExtraModifiers);
        settings.PropertyChanged += Settings_OnPropertyChanged;
    }

    private void MicOnMuteStatusChanged(object? sender, bool e) => MuteStatusChanged?.Invoke(this, e);

    private void Settings_OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        var settings = (Settings)sender!;
        if (nameof(Settings.MuteShortcut).Equals(e.PropertyName, StringComparison.Ordinal)
            || nameof(Settings.IgnoreExtraModifiers).Equals(e.PropertyName, StringComparison.Ordinal))
        {
            UpdateMuteShortcut(settings.MuteShortcut, settings.IgnoreExtraModifiers);
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

    private void UpdateMuteShortcut(Shortcut newShortcut, bool ignoreExtraModifiers)
    {
        if (_hotkey is not null && _hotkey.Shortcut == newShortcut && _hotkey.IgnoresExtraModifiers == ignoreExtraModifiers) return;
        
        _hotkey?.Dispose();

        if (newShortcut == default)
        {
            _hotkey = null;
            return;
        }

        const int tries = 5;
        for (int i = 0; i < tries; i++)
        {
            try
            {
                _hotkey = _hotkeyFactory.Register(newShortcut, ignoreExtraModifiers, _getMainWindowHandle.Value()!.Handle);
                break;
            }
            catch (Win32Exception ex) when (ex.NativeErrorCode == 1409) // Hot key is already registered
            {
                if (i == tries - 1) throw;
            }
        }

        _hotkey!.Pressed += OnHotkeyPressed;
        _logger.LogInformation("Registered new hotkey: {Shortcut}, IgnoreExtraModifiers: {IgnoreExtraModifiers}", _hotkey.Shortcut, ignoreExtraModifiers);
    }

    public void Dispose() => _hotkey?.Dispose();
}
