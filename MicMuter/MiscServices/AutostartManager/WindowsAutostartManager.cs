using System;
using System.IO;
using System.Reflection;
using Microsoft.Win32;
using Microsoft.Win32.TaskScheduler;

namespace MicMuter.MiscServices.AutostartManager;

#pragma warning disable CA1416 // (Validate platform compatibility)

internal sealed class WindowsAutostartManager : IAutostartManager
{
    private static string ExePath => Path.ChangeExtension(Assembly.GetEntryAssembly()!.Location, ".exe");
    
    public void SetAutostart(bool value, bool elevated)
    {
        if (elevated) SetStartupAsAdminTask(value);
        else SetStartupKey(value);
    }
    
    private static void SetStartupKey(bool value)
    {
        const string path = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
        RegistryKey key = Registry.CurrentUser.OpenSubKey(path, true) ?? throw new InvalidOperationException("Opening registry key failed.");

        if (value)
        {
            key.SetValue(nameof(MicMuter), ExePath);
            Helpers.DebugWriteLine("Created startup registry key");
            return;
        }

        if (key.GetValue(nameof(MicMuter)) is null) return;
        
        key.DeleteValue(nameof(MicMuter));
        Helpers.DebugWriteLine("Deleted startup registry key");
    }

    private static void SetStartupAsAdminTask(bool value)
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
                return;
            }

            if (task is null)
            {
                taskDef = TaskService.Instance.NewTask();
                taskDef.RegistrationInfo.Author = nameof(MicMuter);
                taskDef.RegistrationInfo.Description = "Automatically runs MicMuter with administrator privileges on startup.";
                taskDef.Principal.RunLevel = TaskRunLevel.Highest;
                taskDef.Triggers.Add(new LogonTrigger { UserId = Environment.UserName });
                taskDef.Actions.Add(ExePath);
                task = TaskService.Instance.RootFolder.RegisterTaskDefinition(taskName, taskDef);
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
