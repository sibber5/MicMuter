using System;
using System.Collections.Generic;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using MicMuter.AppSettings;
using MicMuter.Audio;
using MicMuter.Hotkeys;

namespace MicMuter.MainWindow;

public sealed partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty]
    private IReadOnlyList<IMicDevice> _mics;

    [ObservableProperty]
    private Shortcut _shortcut;
    partial void OnShortcutChanged(Shortcut value)
    {
        if (_updateSettings) Settings.MuteShortcut = value;
    }
    
    public Settings Settings { get; }
    
    private readonly IMicDeviceManager _micDeviceManager;

    internal bool _updateSettings = true;

    public MainWindowViewModel(IMicDeviceManager micDeviceManager, Settings settings)
    {
        _micDeviceManager = micDeviceManager;

        Settings = settings;
        Settings.PropertyChanged += Settings_OnPropertyChanged;

        _mics = _micDeviceManager.GetMicDevices();
        _shortcut = Settings.MuteShortcut;
    }

    private void Settings_OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        _updateSettings = false;
        if (nameof(Settings.MuteShortcut).Equals(e.PropertyName, StringComparison.Ordinal))
        {
            Shortcut = Settings.MuteShortcut;
        }
        _updateSettings = true;
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