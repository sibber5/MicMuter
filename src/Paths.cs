using System;
using System.IO;

namespace MicMuter;

internal static class Paths
{
    public static readonly string ExePath = Path.Join(AppContext.BaseDirectory, $"{nameof(MicMuter)}.exe");
    public static readonly string SaveFileDir = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "/MicMuter");
    public static readonly string SaveFilePath = Path.Join(SaveFileDir, "/UserSettings.json");
    public static readonly string LogPath = Path.Join(SaveFileDir, $"/Logs/log.txt");
}
