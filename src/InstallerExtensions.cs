using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using MicMuter.MiscServices.AutostartManager;
using Microsoft.Extensions.DependencyInjection;
using Velopack;

namespace MicMuter;

internal static class InstallerExtensions
{
    public static VelopackApp ConfigureInstaller(this VelopackApp app)
    {
        // IMPORTANT: This callback is only supported on Windows.
        app.WithBeforeUninstallFastCallback(v =>
        {
            ProcessStartInfo info = new()
            {
                FileName = Paths.ExePath,
                Verb = "runas",
                UseShellExecute = true,
                Arguments = Program.CleanupArg,
            };
            
            try
            {
                Process.Start(info)!.WaitForExit();
            }
            catch (Win32Exception ex) when (ex.NativeErrorCode == 1223) // The operation (UAC prompt in this case) was canceled by the user.
            {
                info.Verb = "";
                Process.Start(info)?.WaitForExit();
            }
        });

        return app;
    }
    
    public static void CleanupInstallationAndExit()
    {
        var services = new ServiceCollection().AddPlatformSpecificServices().BuildServiceProvider();
        
        bool success = true;
        IAutostartManager? autostartManager = services.GetService<IAutostartManager>();
        
        try
        {
            autostartManager?.SetAutostart(false, false);
        }
        catch
        {
            success = false;
        }
        
        try
        {
            autostartManager?.SetAutostart(false, true);
        }
        catch
        {
            success = false;
        }
        
        try
        {
            Directory.Delete(Paths.SaveFileDir, true);
        }
        catch
        {
            success = false;
        }
        finally
        {
            services.Dispose();
        }
        
        Environment.Exit(success ? 0 : 1);
    }
}
