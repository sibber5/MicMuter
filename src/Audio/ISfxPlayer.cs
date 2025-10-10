using System;
using System.Threading.Tasks;

namespace MicMuter.Audio;

public interface ISfxPlayer
{
    Task LoadFromFile(string filePath);
    Task LoadFromResource(Uri resourceUri);
    void Unload();
    void Play();
}
