using System;
using Avalonia.Platform;
using MicMuter;
using MicMuter.AppSettings;
using MicMuter.Audio;
using MicMuter.Audio.Windows;
using MicMuter.DummyServices;
using MicMuter.Hotkeys;
using MicMuter.Hotkeys.Windows;
using MicMuter.MainWindow;
using MicMuter.MiscServices;
using MicMuter.MiscServices.AutostartManager;
using MicMuter.MiscServices.ElevatedCheck;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAppLogging(this IServiceCollection services)
    {
        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .Enrich.WithThreadId()
#if DEBUG
            .WriteTo.File(Paths.LogPath, restrictedToMinimumLevel: LogEventLevel.Information, rollingInterval: RollingInterval.Day, retainedFileCountLimit: 14, rollOnFileSizeLimit: true, buffered: false,
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] [Thread {ThreadId}] [{SourceContext}] {Message:lj}{NewLine}{Exception}")
#endif
            .WriteTo.Debug(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [Thread {ThreadId}] [{SourceContext}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();
        
        return services.AddLogging(builder => builder.AddSerilog(dispose: true));
    }
    
    public static IServiceCollection AddAppServices(this IServiceCollection services)
    {
        services.AddTransient(typeof(LazyService<>));

        services.AddSingleton<SettingsSerializer>();
        services.AddSingleton<Settings>();
        services.AddSingleton<MicMuterService>();
        
        services.AddPlatformSpecificServices();
        // services.AddDummyServices();
        
        services.AddSingleton<TrayIconViewModel>();
        services.AddSingleton<MainWindowViewModel>();
        
        services.AddSingleton<MainWindow>();

        services.AddSingleton<Func<IPlatformHandle?>>(provider => provider.GetRequiredService<MainWindow>().TryGetPlatformHandle);
        
        return services;
    }
    
    public static IServiceCollection AddPlatformSpecificServices(this IServiceCollection services)
    {
#if WINDOWS
        services.AddWindowsServices();
#endif
        return services;
    }
    
    private static IServiceCollection AddWindowsServices(this IServiceCollection services)
    {
        services.AddSingleton<IMicDeviceManager, WindowsMicDeviceManager>();
        services.AddSingleton<IGlobalHotkeyFactory, WindowsGlobalHotkeyFactory>();
        services.AddSingleton<IElevatedChecker, WindowsElevatedChecker>();
        services.AddSingleton<IAutostartManager, WindowsAutostartManager>();
        return services;
    }
    
#if DEBUG
    private static IServiceCollection AddDummyServices(this IServiceCollection services)
    {
        services.AddSingleton<IMicDeviceManager, DummyMicDeviceManager>();
        services.AddSingleton<IGlobalHotkeyFactory, DummyGlobalHotkeyFactory>();
        services.AddSingleton<IElevatedChecker, DummyElevatedChecker>();
        services.AddSingleton<IAutostartManager, DummyAutostartManager>();
        return services;
    }
#endif
}