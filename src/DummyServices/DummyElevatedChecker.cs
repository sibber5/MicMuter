#if DEBUG
using MicMuter.MiscServices.ElevatedCheck;

namespace MicMuter.DummyServices;

internal sealed class DummyElevatedChecker : IElevatedChecker
{
    public bool IsElevated { get; } = false;
}
#endif
