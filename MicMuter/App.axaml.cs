using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Threading;
using MicMuter.Audio;
using MicMuter.Audio.Windows;
using MicMuter.Hotkeys;
using MicMuter.MainWindow;
using MicMuter.AppSettings;
using MicMuter.Dialogs;
using MicMuter.Hotkeys.Windows;
using MicMuter.MiscServices;
using MicMuter.MiscServices.AutostartManager;
using MicMuter.MiscServices.ElevatedCheck;
using Microsoft.Extensions.DependencyInjection;

namespace MicMuter;

public class App : Application
{
    private readonly IServiceProvider _services;

    private Window _mainWindow = null!;

    public App()
    {
        _services = ConfigureServicesForWindows();
    }
    
    private void SettingsMenuItem_OnClick(object? sender, EventArgs e)
    {
        _mainWindow.Show();
    }
    
    private void ExitMenuItem_OnClick(object? sender, EventArgs e)
    {
        ((IClassicDesktopStyleApplicationLifetime)ApplicationLifetime!).Shutdown();
    }
    
    private IServiceProvider ConfigureServicesForWindows()
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
    
    public override void Initialize()
    {
        DataContext = _services.GetRequiredService<TrayIconViewModel>();
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();
            
            desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            
            _ = _services.GetRequiredService<SettingsSerializer>().Load().ContinueWith((t, args) =>
            {
                if (t.IsCompletedSuccessfully)
                {
                    var (services, dispatcher) = ((IServiceProvider, Dispatcher))args!;
                    if (!t.Result.StartMinimized) dispatcher.Post(services.GetRequiredService<MainWindow.MainWindow>().Show, DispatcherPriority.Loaded);
                }
                else
                {
                    Debug.WriteLine("Failed to load settings.");
                    if (t.IsFaulted) Debug.WriteLine($"\nUnhandled exception while loading settings: {t.Exception?.Message}\n");
                }
            }, (_services, Dispatcher.UIThread));

            _services.GetRequiredService<Settings>().SettingUpdateFailed += OnSettingUpdateFailed;
            
            _mainWindow = _services.GetRequiredService<MainWindow.MainWindow>();
        }

        base.OnFrameworkInitializationCompleted();
    }

    // ReSharper disable once AsyncVoidMethod
    private async void OnSettingUpdateFailed(object? sender, ChangeFailReason e)
    {
        TryGetResource("ShieldIcon", null, out var acceptIcon);

        var v = new DialogWindow(
            "Elevated privileges required",
            "Administrator privileges are required to change this setting.\n\nRestart as administrator?",
            "Restart",
            "Cancel",
            acceptIcon: (IImage)acceptIcon!);
        
        var result = await v.ShowDialog<DialogResult>(_mainWindow);
        if (result == DialogResult.Accept) await RestartElevated();
    }

    private async ValueTask RestartElevated()
    {
        // Hotkey registration will fail if there's already a hotkey registered
        // but we cant dispose the hotkey here in case the user cancels the restart.
        // So instead to work around this, we attempt to register a couple of times on startup,
        // with a small delay between each registration
        // _services.GetRequiredService<MicMuterService>().Dispose();

        #if WINDOWS
        
        ProcessStartInfo info = new()
        {
            FileName = Path.ChangeExtension(Assembly.GetEntryAssembly()!.Location, ".exe"),
            Verb = "runas",
            UseShellExecute = true,
        };

        try
        {
            Process.Start(info);
        }
        catch (Win32Exception ex) when (ex.NativeErrorCode == 1223) // The operation (UAC prompt in this case) was canceled by the user.
        {
            await new DialogWindow("Error", "The operation was canceled by the user.", null, "Ok").ShowDialog(_mainWindow);
        }
        
        #else
        throw new NotImplementedException();
        #endif
        
        ((IClassicDesktopStyleApplicationLifetime)ApplicationLifetime!).Shutdown();
    }
    
    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}