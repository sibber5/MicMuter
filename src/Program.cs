using Avalonia;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia.Platform;
using MicMuter.AppSettings;
using MicMuter.Audio;
using MicMuter.Audio.Windows;
using MicMuter.Dialogs;
using MicMuter.Hotkeys;
using MicMuter.Hotkeys.Windows;
using MicMuter.MainWindow;
using MicMuter.MiscServices;
using MicMuter.MiscServices.AutostartManager;
using MicMuter.MiscServices.ElevatedCheck;
using Microsoft.Extensions.DependencyInjection;
using NAudio.CoreAudioApi;
using Velopack;

namespace MicMuter;

internal sealed class Program
{
    private static IServiceProvider? _services;
    
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        const string cleanupArg = "--uninstall-cleanup";
        
        if (args.Length > 0 && cleanupArg.Equals(args[0], StringComparison.Ordinal))
        {
            CleanupInstallationAndExit();
        }
        
        VelopackApp.Build()
            .WithBeforeUninstallFastCallback(v =>
            {
                #if WINDOWS
                ProcessStartInfo info = new()
                {
                    FileName = App.ExePath,
                    Verb = "runas",
                    UseShellExecute = true,
                    Arguments = cleanupArg,
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
                #endif
            })
            .Run();
        
        TaskScheduler.UnobservedTaskException += TaskScheduler_OnUnobservedTaskException;
        
        try
        {
            _services ??= ConfigureServices();
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            OnUnhandledException(ex);
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure(() => new App(_services!))
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();

    private static void CleanupInstallationAndExit()
    {
        // ReSharper disable EmptyGeneralCatchClause
            
        _services = ConfigureServices();

        bool success = true;
        IAutostartManager? autostartManager = _services.GetService<IAutostartManager>();

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
            File.Delete(SettingsSerializer.SaveFilePath);
            Directory.Delete(SettingsSerializer.SaveFileDir);
        }
        catch
        {
            success = false;
        }
        
        Environment.Exit(success ? 0 : 1);
    }
    
    private static IServiceProvider ConfigureServices()
    {
        return ConfigureServicesForWindows();
    }
    
    private static IServiceProvider ConfigureServicesForWindows()
    {
        ServiceCollection services = new();

        services.AddTransient(typeof(LazyService<>));

        services.AddSingleton<SettingsSerializer>();
        services.AddSingleton<Settings>();
        services.AddSingleton<MicMuterService>();
        
        services.AddSingleton<IMicDeviceManager, WindowsMicDeviceManager>();
        services.AddSingleton<IGlobalHotkeyFactory, WindowsGlobalHotkeyFactory>();
        services.AddSingleton<IElevatedChecker, WindowsElevatedChecker>();
        services.AddSingleton<IAutostartManager, WindowsAutostartManager>();

        services.AddSingleton<TrayIconViewModel>();
        services.AddSingleton<MainWindowViewModel>();
        
        services.AddSingleton<MainWindow.MainWindow>();

        services.AddSingleton<Func<IPlatformHandle?>>(provider => provider.GetRequiredService<MainWindow.MainWindow>().TryGetPlatformHandle);
        
        return services.BuildServiceProvider();
    }
    
    private static void TaskScheduler_OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        OnUnhandledException(e.Exception, "Unobserved task exception:");
    }

    public static void OnUnhandledException(Exception ex, string? bodyInfo = null)
    {
        MessageBoxError($"{bodyInfo}{Environment.NewLine}{ex.Message}", "Unhandled Exception");
    }
    
    private static int MessageBoxError(string text, string title) => MessageBox(nint.Zero, text, title, 0x000010u);
    
    [DllImport("USER32.dll", ExactSpelling = true, EntryPoint = "MessageBoxW", CharSet = CharSet.Unicode)]
    static extern int MessageBox(nint hWnd, string lpText, string lpCaption, uint uType);
}