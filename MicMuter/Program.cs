using Avalonia;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace MicMuter;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        TaskScheduler.UnobservedTaskException += TaskScheduler_OnUnobservedTaskException;
        try
        {
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            OnUnhandledException(ex);
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();


    private static void TaskScheduler_OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        OnUnhandledException(e.Exception);
    }

    private static void OnUnhandledException(Exception ex)
    {
        MessageBoxError($"An unhandled exception occurred: {ex.Message}", "Unhandled Exception");
    }
    
    public static int MessageBoxError(string text, string title) => MessageBox(nint.Zero, text, title, 0x000010u);
    
    [DllImport("USER32.dll", ExactSpelling = true, EntryPoint = "MessageBoxW", CharSet = CharSet.Unicode)]
    static extern int MessageBox(nint hWnd, string lpText, string lpCaption, uint uType);
}