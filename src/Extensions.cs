using System;

namespace MicMuter;

internal static class Extensions
{
    public static string To3PartVersion(this Version? version) => version is null ? "UNVERSIONED" : $"v{version.Major}.{version.Minor}.{version.Build}";
}
