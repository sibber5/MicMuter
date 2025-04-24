using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using MicMuter.AppSettings;
using MicMuter.Dialogs;
using Microsoft.Extensions.DependencyInjection;

namespace MicMuter;

public class App(IServiceProvider services) : Application
{
    public static string ExePath { get; } = Path.Join(AppContext.BaseDirectory, $"{nameof(MicMuter)}.exe");

    private Window _mainWindow = null!;

    private void SettingsMenuItem_OnClick(object? sender, EventArgs e)
    {
        _mainWindow.Show();
        _mainWindow.WindowState = WindowState.Normal;
        _mainWindow.Activate();
    }
    
    private void ExitMenuItem_OnClick(object? sender, EventArgs e)
    {
        ((IClassicDesktopStyleApplicationLifetime)ApplicationLifetime!).Shutdown();
    }

    public static void ThrowOnMainThread<T>(T ex) where T : Exception
    {
        Dispatcher.UIThread.Post(() => throw ex);
    }
    
    public override void Initialize()
    {
        DataContext = services.GetRequiredService<TrayIconViewModel>();
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
            
            LoadSettingsAsync(services.GetRequiredService<SettingsSerializer>());
            
            services.GetRequiredService<Settings>().SettingUpdateFailed += OnSettingUpdateFailed;
            
            _mainWindow = services.GetRequiredService<MainWindow.MainWindow>();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private async void LoadSettingsAsync(SettingsSerializer settingsSerializer)
    {
        try
        {
            var settings = await settingsSerializer.Load();
            if (!settings.StartMinimized) Dispatcher.UIThread.Post(services.GetRequiredService<MainWindow.MainWindow>().Show, DispatcherPriority.Loaded);
        }
        catch (Exception ex)
        {
            Helpers.DebugWriteLine("Failed to load settings.");
            Dispatcher.UIThread.Post(() =>
            {
                Program.OnUnhandledException(ex, "Unhandled exception while loading settings:");
                ((IClassicDesktopStyleApplicationLifetime)ApplicationLifetime!).Shutdown();
            });
        }
    }

    // ReSharper disable once AsyncVoidMethod
    private async void OnSettingUpdateFailed(object? sender, ChangeFailReason e)
    {
        switch (e)
        {
            case ChangeFailReason.ElevatedPermissionsRequired:
                await PromptToRestartElevated();
                break;
            
            case ChangeFailReason.UnhandledException:
                ThrowOnMainThread(new InvalidOperationException("Unhandled exception while loading settings."));
                break;
            
            default:
                throw new NotImplementedException();
        }
    }

    private async Task PromptToRestartElevated()
    {
        TryGetResource("ShieldIcon", null, out var acceptIcon);

        var v = new DialogWindow(
            "Elevated privileges required",
            "Administrator privileges are required to change this setting.\n\nRestart as administrator?",
            "Restart",
            "Cancel",
            acceptIcon: (IImage)acceptIcon!);

        try
        {
            var result = await v.ShowDialog<DialogResult>(_mainWindow);
            if (result == DialogResult.Accept) await RestartElevated();
        }
        catch (Exception ex)
        {
            ThrowOnMainThread(ex);
        }
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
            FileName = ExePath,
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
#pragma warning disable IL2026
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
#pragma warning restore IL2026
    }
}