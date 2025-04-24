using System.Diagnostics;
using System.Threading;
using Avalonia.Media;

namespace MicMuter;

internal static class Helpers
{
    [Conditional("DEBUG")]
    public static void DebugWriteLine(string message)
    {
        StackFrame frame = new(1);
#pragma warning disable IL2026
        string typeName = frame.GetMethod()?.DeclaringType?.Name ?? "";
#pragma warning restore IL2026
        Debug.WriteLine($"[{typeName}] {message}");
    }
    
    public static (double Width, double Height) GetRenderedTextDimensions(string text, FontFamily fontFamily, double fontSize, double? maxWidth = null)
    {
        var typeface = new Typeface(fontFamily);
        FormattedText formatted = new(
            text,
            Thread.CurrentThread.CurrentUICulture,
            FlowDirection.LeftToRight,
            typeface,
            fontSize,
            null);

        if (maxWidth is not null)
        {
            formatted.MaxTextWidth = maxWidth.Value;
        }
        
        return (formatted.Width, formatted.Height);
    }
}
