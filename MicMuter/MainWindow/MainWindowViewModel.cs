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

    public Settings Settings { get; }
    private readonly IMicDeviceManager _micDeviceManager;

    public MainWindowViewModel(IMicDeviceManager micDeviceManager, Settings settings)
    {
        _micDeviceManager = micDeviceManager;
        
        Settings = settings;
        
        _mics = _micDeviceManager.GetMicDevices();
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