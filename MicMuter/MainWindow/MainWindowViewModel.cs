using System.Collections.Generic;
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
    partial void OnSelectedDeviceChanged(IMicDevice? value) => _settings.MicDevice = value;
    
    [ObservableProperty]
    private Shortcut _shortcut;
    partial void OnShortcutChanged(Shortcut value) => _settings.MuteShortcut = value;
    
    private readonly Settings _settings;
    private readonly IMicDeviceManager _micDeviceManager;

    public MainWindowViewModel(IMicDeviceManager micDeviceManager, Settings settings)
    {
        _settings = settings;
        _micDeviceManager = micDeviceManager;
        _mics = _micDeviceManager.GetMicDevices();
        SelectedDevice = _micDeviceManager.GetDefaultMicDevice();
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