using System;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using Microsoft.Win32.TaskScheduler;

namespace MicMuter.MiscServices.AutostartManager;

#pragma warning disable CA1416 // (Validate platform compatibility)

internal sealed class WindowsAutostartManager(ILogger<WindowsAutostartManager> logger) : IAutostartManager
{
    public void SetAutostart(bool value, bool elevated)
    {
        if (elevated) SetStartupAsAdminTask(value);
        else SetStartupKey(value);
    }
    
    private void SetStartupKey(bool value)
    {
        const string path = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
        RegistryKey key = Registry.CurrentUser.OpenSubKey(path, true) ?? throw new InvalidOperationException("Opening registry key failed.");
        
        if (value)
        {
            key.SetValue(nameof(MicMuter), Paths.ExePath);
            logger.LogInformation("Created startup registry key");
            return;
        }
        
        if (key.GetValue(nameof(MicMuter)) is null) return;
        
        key.DeleteValue(nameof(MicMuter));
        logger.LogInformation("Deleted startup registry key");
    }
    
    private void SetStartupAsAdminTask(bool value)
    {
        string taskName = $"{nameof(MicMuter)} - Run on startup for {Environment.UserName}";
        TaskDefinition? taskDef = null;
        Task? task = null;
        try
        {
            task = TaskService.Instance.GetTask($@"\{taskName}");
            
            if (!value)
            {
                task?.Folder.DeleteTask(taskName, false);
                logger.LogInformation("Deleted startup task");
                return;
            }
            
            if (task is null)
            {
                taskDef = TaskService.Instance.NewTask();
                taskDef.RegistrationInfo.Author = nameof(MicMuter);
                taskDef.RegistrationInfo.Description = "Automatically runs MicMuter with administrator privileges on startup.";
                taskDef.Principal.RunLevel = TaskRunLevel.Highest;
                taskDef.Triggers.Add(new LogonTrigger { UserId = Environment.UserName });
                taskDef.Actions.Add(Paths.ExePath);
                
                // some of these are on by default for some reason,
                // set all to off in case of some breaking change in the future
                taskDef.Settings.RunOnlyIfIdle = false;
                taskDef.Settings.IdleSettings.StopOnIdleEnd = false;
                taskDef.Settings.IdleSettings.RestartOnIdle = false;
                taskDef.Settings.DisallowStartIfOnBatteries = false;
                taskDef.Settings.StopIfGoingOnBatteries = false;
                taskDef.Settings.WakeToRun = false;
                taskDef.Settings.RunOnlyIfNetworkAvailable = false;
                
                task = TaskService.Instance.RootFolder.RegisterTaskDefinition(taskName, taskDef);
                logger.LogInformation("Created startup task");
            }
            else
            {
                logger.LogInformation("Startup task already exists");
            }
            
            task.Enabled = true;
        }
        finally
        {
            task?.Dispose();
            taskDef?.Dispose();
        }
    }
}
