using System;
using CommunityToolkit.Mvvm.ComponentModel;
using MicMuter.Audio;
using MicMuter.Hotkeys;
using MicMuter.MiscServices;
using MicMuter.MiscServices.AutostartManager;
using MicMuter.MiscServices.ElevatedCheck;
using Microsoft.Extensions.Logging;

namespace MicMuter.AppSettings;

public sealed partial class Settings(IAutostartManager autostartManager, IElevatedChecker elevatedChecker, LazyService<SettingsSerializer> serializer, ILogger<Settings> logger) : ObservableObject
{
    public event EventHandler<ChangeFailReason>? SettingUpdateFailed; 
    
    [ObservableProperty]
    private IMicDevice? _micDevice;
    
    [ObservableProperty]
    private Shortcut _muteShortcut;
    
    [ObservableProperty]
    private bool _ignoreExtraModifiers = true;
    
    private bool _runOnStartup;
    public bool RunOnStartup
    {
        get => _runOnStartup;
        set
        {
            bool oldValue = _runOnStartup;
            bool revert = UpdateRunOnStartup(value, StartElevated, oldValue, StartElevated);
            SetProperty(ref _runOnStartup, value);
            if (revert) SetProperty(ref _runOnStartup, oldValue);
        }
    }
    
    private bool _startElevated;

    public bool StartElevated
    {
        get => _startElevated;
        set
        {
            bool oldValue = _startElevated;
            bool revert = UpdateRunOnStartup(RunOnStartup, value, RunOnStartup, oldValue);
            SetProperty(ref _startElevated, value);
            if (revert) SetProperty(ref _startElevated, oldValue);
        }
    }
    
    [ObservableProperty]
    private bool _startMinimized;

    /// <returns><see langword="true"/> if the properties should be reverted, otherwise <see langword="false"/>.</returns>
    private bool UpdateRunOnStartup(bool enable, bool elevated, bool oldEnable, bool oldElevated)
    {
        if (serializer.Value.IsLoadingSettings) return false;
        
        if (!elevatedChecker.IsElevated && (elevated || oldElevated))
        {
            logger.LogWarning("Updating Run on Startup failed. Reason: {ChangeFailReason}", ChangeFailReason.ElevatedPermissionsRequired);
            SettingUpdateFailed?.Invoke(this, ChangeFailReason.ElevatedPermissionsRequired);
            return true;
        }

        // if this is true it means RunOnStartup was changed, otherwise it means StartElevated was changed
        if (enable != oldEnable)
        {
            autostartManager.SetAutostart(enable, elevated);
            return false;
        }
        
        // from here onwards, StartElevated was changed, and RunOnStartup was not changed

        if (!enable) return false;
        
        autostartManager.SetAutostart(false, oldElevated);
        autostartManager.SetAutostart(true, elevated);
        
        return false;
    }

    public void Deconstruct(out IMicDevice? micDevice, out Shortcut muteShortcut, out bool ignoreExtraModifiers, out bool runOnStartup, out bool startElevated, out bool startMinimized)
    {
        micDevice = MicDevice;
        muteShortcut = MuteShortcut;
        ignoreExtraModifiers = IgnoreExtraModifiers;
        runOnStartup = RunOnStartup;
        startElevated = StartElevated;
        startMinimized = StartMinimized;
    }
}

public enum ChangeFailReason
{
    None = 0,
    ElevatedPermissionsRequired,
    UnhandledException,
}
