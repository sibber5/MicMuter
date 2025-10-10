using System;
using System.Threading.Tasks;

namespace MicMuter.Audio;

public interface ISfxPlayer
{
    Task LoadFromFile(string filePath);

    Task LoadFromResource(Uri resourceUri);

    void Unload();

    void Play();

    void Stop();

    /// <summary>
    /// Gets or sets the volume to a value between 0.0 and 1.0.
    /// </summary>
    double Volume { get; set; }
}
