using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Markup.Xaml;
using MicMuter.Audio;
using MicMuter.Hotkeys;
using MicMuter.MainWindow;
using Microsoft.Extensions.DependencyInjection;

namespace MicMuter;

public partial class App : Application
{
    private readonly IServiceProvider _services;

    public App()
    {
        _services = ConfigureServices();
    }
    
    private IServiceProvider ConfigureServices()
    {
        ServiceCollection services = new();

        services.AddSingleton<Settings>();
        services.AddSingleton<MicMuterService>();
        
        services.AddSingleton<IMicDeviceManager, WindowsMicDeviceManager>();
        services.AddSingleton<IGlobalHotkeyFactory, WindowsGlobalHotkeyFactory>();

        services.AddSingleton<TrayIconViewModel>();
        services.AddSingleton<MainWindowViewModel>();
        
        services.AddSingleton<MainWindow.MainWindow>();
        
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
            desktop.MainWindow = _services.GetRequiredService<MainWindow.MainWindow>();
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