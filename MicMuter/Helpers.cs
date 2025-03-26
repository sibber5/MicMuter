#if DEBUG
using System.Diagnostics;

namespace MicMuter;

internal static class Helpers
{
    [Conditional("DEBUG")]
    public static void DebugWriteLine(string message)
    {
        StackFrame frame = new(1);
        string typeName = frame.GetMethod()?.DeclaringType?.Name ?? "";
        Debug.WriteLine($"[{typeName}] {message}");
    }
}
#endif