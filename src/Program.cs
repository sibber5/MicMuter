using Avalonia;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Velopack;

namespace MicMuter;

internal sealed class Program
{
    public const string CleanupArg = "--uninstall-cleanup";
    
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        if (args.Length > 0 && CleanupArg.Equals(args[0], StringComparison.Ordinal))
        {
            InstallerExtensions.CleanupInstallationAndExit();
        }
        
        VelopackApp.Build()
            .ConfigureInstaller()
            .Run();
        
        TaskScheduler.UnobservedTaskException += TaskScheduler_OnUnobservedTaskException;
        
#if !DEBUG
        try
        {
#endif
            ServiceCollection services = new();
            
            services.AddAppServices();
            services.AddAppLogging();
            
            var serviceProvider = services.BuildServiceProvider();
            StaticLogger.LoggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            
            Log.Information("Starting application...");
            BuildAvaloniaApp(serviceProvider).StartWithClassicDesktopLifetime(args);
#if !DEBUG
        }
        catch (Exception ex)
        {
            OnUnhandledException(ex);
        }
#endif
    }
    
    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp(ServiceProvider serviceProvider)
        => AppBuilder.Configure(() => new App(serviceProvider))
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
    
    private static void TaskScheduler_OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        OnUnhandledException(e.Exception, "Unobserved task exception:");
    }
    
    public static void OnUnhandledException<T>(T ex, string? bodyInfo = null) where T : Exception
    {
        Log.Fatal(ex, "Unhandled exception");
        // Debugger.BreakForUserUnhandledException(ex);
        Debugger.Break();
        Dispatcher.UIThread.Invoke(() => MessageBoxError($"{bodyInfo}{Environment.NewLine}{ex.Message}", $"Unhandled Exception - Please submit bug report"));
    }
    
    private static int MessageBoxError(string text, string title) => MessageBox(nint.Zero, text, title, 0x000010u);
    
    [DllImport("USER32.dll", ExactSpelling = true, EntryPoint = "MessageBoxW", CharSet = CharSet.Unicode)]
    static extern int MessageBox(nint hWnd, string lpText, string lpCaption, uint uType);
}