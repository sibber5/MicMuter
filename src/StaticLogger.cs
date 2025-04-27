using Microsoft.Extensions.Logging;

namespace MicMuter;

internal static class StaticLogger
{
    public static ILoggerFactory? LoggerFactory { get; set; }
    public static ILogger<T> CreateLogger<T>() => LoggerFactory!.CreateLogger<T>();
}
