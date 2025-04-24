using System.Security.Principal;

namespace MicMuter.MiscServices.ElevatedCheck;

#pragma warning disable CA1416 // (Validate platform compatibility)

internal sealed class WindowsElevatedChecker : IElevatedChecker
{
    public bool IsElevated => _isElevated;
    
    private static readonly bool _isElevated;

    static WindowsElevatedChecker()
    {
        using var id = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(id);
        _isElevated = principal.IsInRole(WindowsBuiltInRole.Administrator);
    }
}
