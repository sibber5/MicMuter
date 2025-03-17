using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using MicMuter.Audio;

namespace MicMuter.ViewModels;

internal sealed partial class MainWindowViewModel(IMicDeviceManager micDeviceManager) : ObservableObject
{
    private IReadOnlyList<MicDevice> _mics = null!;

    public IEnumerable<string> MicFriendlyNames => (_mics = micDeviceManager.GetMicDevices()).Select(x => x.FriendlyName);

    [ObservableProperty]
    private int _selectedIndex = -1;

    public void NotifyPropertyChanged(string propertyName)
    {
        bool b = false;
        SetProperty(ref b, true, propertyName: propertyName);
    }
}