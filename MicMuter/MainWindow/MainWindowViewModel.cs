using System;
using System.Collections.Generic;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using MicMuter.Audio;
using MicMuter.Hotkeys;

namespace MicMuter.MainWindow;

public sealed partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty]
    private IReadOnlyList<IMicDevice> _mics;

    [ObservableProperty]
    private IMicDevice? _selectedDevice;
    partial void OnSelectedDeviceChanged(IMicDevice? value)
    {
        if (!_updatingFromSettings) _settings.MicDevice = value;
    }

    [ObservableProperty]
    private Shortcut _shortcut;
    partial void OnShortcutChanged(Shortcut value)
    {
        if (!_updatingFromSettings) _settings.MuteShortcut = value;
    }

    private readonly Settings _settings;
    private readonly IMicDeviceManager _micDeviceManager;

    private bool _updatingFromSettings;
    
    public MainWindowViewModel(IMicDeviceManager micDeviceManager, Settings settings)
    {
        _micDeviceManager = micDeviceManager;
        
        _settings = settings;
        _settings.PropertyChanged += Settings_OnPropertyChanged;
        
        _mics = _micDeviceManager.GetMicDevices();
        _selectedDevice = _settings.MicDevice;
        _shortcut = _settings.MuteShortcut;
    }

    private void Settings_OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        _updatingFromSettings = true;
        if (nameof(Settings.MuteShortcut).Equals(e.PropertyName, StringComparison.Ordinal))
        {
            Shortcut = _settings.MuteShortcut;
        }
        else if (nameof(Settings.MicDevice).Equals(e.PropertyName, StringComparison.Ordinal))
        {
            SelectedDevice = _settings.MicDevice;
        }
        _updatingFromSettings = false;
    }

    public void RefreshDeviceList()
    {
        Mics = _micDeviceManager.GetMicDevices();
    }
    
    public void NotifyPropertyChanged(string propertyName)
    {
        bool b = false;
        SetProperty(ref b, true, propertyName: propertyName);
    }
}