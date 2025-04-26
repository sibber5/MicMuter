#if DEBUG
using MicMuter.MiscServices.AutostartManager;

namespace MicMuter.DummyServices;

internal sealed class DummyAutostartManager : IAutostartManager
{
    public void SetAutostart(bool value, bool elevated) {}
}
#endif
