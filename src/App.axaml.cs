using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
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
using Microsoft.Extensions.Logging;

namespace MicMuter;

public class App(ServiceProvider services) : Application
{
    public App() : this(null!) => throw new UnreachableException();
    
    private ILogger<App> _logger = null!;
    private Window _mainWindow = null!;
    private Window? _aboutWindow;
    
    private void SettingsMenuItem_OnClick(object? sender, EventArgs e)
    {
        _mainWindow.Show();
        _mainWindow.WindowState = WindowState.Normal;
        _mainWindow.Activate();
    }
    
    private async void ReportBugMenuItem_OnClick(object? sender, EventArgs e)
    {
        string logArchiveName = $"MicMuter_Logs_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.zip";
        bool createdArchive = false;
        
        try
        {
            await Task.Run(() => CreateLogArchive(logArchiveName));
            createdArchive = true;
        }
        catch (Exception ex)
        {
            new DialogWindow(ex.Message, @"Failed to create log archive. Please upload logs manually from %AppData%\MicMuter\Logs", null, "Ok").Show();
        }
        
        if (createdArchive)
        {
            new DialogWindow("Log archive", $"Log archive {logArchiveName} created on Desktop.{Environment.NewLine}{Environment.NewLine}Please attach to the bug report.", null, "Ok").Show();
        }
        
        try
        {
            Process.Start(new ProcessStartInfo("https://github.com/sibber5/MicMuter/issues/new/choose") { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            Program.OnUnhandledException(ex);
        }
        
        static void CreateLogArchive(string name)
        {
            string tempDir = Path.Join(Path.GetTempPath(), $"/MicMuterLogs_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}");
            Directory.CreateDirectory(tempDir);
            
            foreach (string file in Directory.GetFiles(Paths.LogDir))
            {
                string dest = Path.Join(tempDir, $"/{Path.GetFileName(file)}");
                File.Copy(file, dest);
            }
            
            ZipFile.CreateFromDirectory(tempDir, Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), $"/{name}"));
            Directory.Delete(tempDir, true);
        }
    }
    
    private void AboutMenuItem_OnClick(object? sender, EventArgs e)
    {
        if (_aboutWindow is null)
        {
            _aboutWindow = new AboutWindow();
            _aboutWindow.Closed += (_, _) => _aboutWindow = null;
        }
        _aboutWindow.Show();
        _aboutWindow.WindowState = WindowState.Normal;
        _aboutWindow.Activate();
    }
    
    private void ExitMenuItem_OnClick(object? sender, EventArgs e)
    {
        ((IClassicDesktopStyleApplicationLifetime)ApplicationLifetime!).Shutdown();
    }
    
    public override void Initialize()
    {
        DataContext = services.GetRequiredService<TrayIconViewModel>();
        AvaloniaXamlLoader.Load(this);
    }
    
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop) throw new NotSupportedException();
        
        // Avoid duplicate validations from both Avalonia and the CommunityToolkit.
        // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
        DisableAvaloniaDataAnnotationValidation();
        
        _logger = services.GetRequiredService<ILogger<App>>();
        
        desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;
        desktop.Exit += (_, _) =>
        {
            // WindowMessageMonitor.Dispose() might get called after the window is freed, that's why RemoveWindowSubclass could fail here, but that's fine.
            if (Dispatcher.UIThread.CheckAccess()) services.Dispose();
            else Dispatcher.UIThread.Invoke(services.Dispose);
        };
        
        LoadSettingsAsync(services.GetRequiredService<SettingsSerializer>());
        
        services.GetRequiredService<Settings>().SettingUpdateFailed += OnSettingUpdateFailed;
        
        _mainWindow = services.GetRequiredService<MainWindow.MainWindow>();
        
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
            _logger.LogError(ex, "Failed to load settings.");
            new DialogWindow("MiMuter - Error", "Failed to load settings.", null, "Ok").Show();
            // instead of shutting down, just show the error dialog and load again without the settings file in order to load the defaults.
            try
            {
                File.Delete(Paths.SaveFilePath);
                var settings = await settingsSerializer.Load();
                if (!settings.StartMinimized) Dispatcher.UIThread.Post(services.GetRequiredService<MainWindow.MainWindow>().Show, DispatcherPriority.Loaded);
            }
            catch (Exception ex2)
            {
                Program.OnUnhandledException(ex2);
                Dispatcher.UIThread.Invoke(() => ((IClassicDesktopStyleApplicationLifetime)ApplicationLifetime!).Shutdown(1));
            }
        }
    }
    
    private async void OnSettingUpdateFailed(object? sender, ChangeFailReason e)
    {
        try
        {
            switch (e)
            {
                case ChangeFailReason.ElevatedPermissionsRequired:
                    await PromptToRestartElevated();
                    break;
                
                case ChangeFailReason.UnhandledException:
                    throw new InvalidOperationException("Unhandled exception while updating settings.");
                
                default:
                    throw new NotImplementedException();
            }
        }
        catch (Exception ex)
        {
            Program.OnUnhandledException(ex);
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
            FileName = Paths.ExePath,
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
