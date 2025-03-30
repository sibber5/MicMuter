using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using Avalonia.Threading;
using MicMuter.Audio;
using MicMuter.Audio.Windows;
using MicMuter.Hotkeys;
using MicMuter.MainWindow;
using MicMuter.AppSettings;
using MicMuter.Hotkeys.Windows;
using MicMuter.MiscServices;
using Microsoft.Extensions.DependencyInjection;

namespace MicMuter;

public class App : Application
{
    private readonly IServiceProvider _services;

    private Window _mainWindow = null!;

    public App()
    {
        _services = ConfigureServices();
    }
    
    private void SettingsMenuItem_OnClick(object? sender, EventArgs e)
    {
        _mainWindow.Show();
    }
    
    private void ExitMenuItem_OnClick(object? sender, EventArgs e)
    {
        ((IClassicDesktopStyleApplicationLifetime)ApplicationLifetime!).Shutdown();
    }
    
    private IServiceProvider ConfigureServices()
    {
        ServiceCollection services = new();

        services.AddTransient(typeof(LazyService<>));

        services.AddSingleton<SettingsSerializer>();
        services.AddSingleton<Settings>();
        services.AddSingleton<MicMuterService>();
        
        services.AddSingleton<IMicDeviceManager, WindowsMicDeviceManager>();
        services.AddSingleton<IGlobalHotkeyFactory, WindowsGlobalHotkeyFactory>();

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

            _mainWindow = _services.GetRequiredService<MainWindow.MainWindow>();
        }

        base.OnFrameworkInitializationCompleted();
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