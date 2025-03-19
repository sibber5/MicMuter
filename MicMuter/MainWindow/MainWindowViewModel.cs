using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using MicMuter.Audio;
using MicMuter.Hotkeys;

namespace MicMuter.MainWindow;

public sealed partial class MainWindowViewModel : ObservableObject
{
    public nint Hwnd { get; internal set; }
    
    private IReadOnlyList<MicDevice> _mics;

    public IEnumerable<string> MicFriendlyNames => (_mics = _micDeviceManager.GetMicDevices()).Select(x => x.FriendlyName);

    [ObservableProperty]
    private int _selectedIndex;
    partial void OnSelectedIndexChanged(int value) => _settings.MicDevice = value > -1 ? _mics[value] : null;
    
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
        var defaultDevice = _micDeviceManager.GetDefaultMicDevice();
        _selectedIndex = _mics.Index().FirstOrDefault(t => t.Item.Id == defaultDevice.Id).Index;
    }

    public void NotifyPropertyChanged(string propertyName)
    {
        bool b = false;
        SetProperty(ref b, true, propertyName: propertyName);
    }
}