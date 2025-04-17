namespace MicMuter.MiscServices.AutostartManager;

public interface IAutostartManager
{
    void SetAutostart(bool value, bool elevated);
}
