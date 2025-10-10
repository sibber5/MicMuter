using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Platform;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace MicMuter.Audio.Windows;

/// <remarks>
/// If <see cref="Play"/> is called while a sound is already playing, the sound will restart from the beginning.
/// This class is highly optimized for frequent, low-latency playback.
/// </remarks>
public sealed class WindowsSfxPlayer : ISfxPlayer, IDisposable
{
    private readonly WasapiOut _outputDevice;
    private readonly SingleSoundSampleProvider _soundProvider;
    private readonly VolumeSampleProvider _volumeProvider;

    private bool _disposed;

    public double Volume
    {
        get => _volumeProvider.Volume / 2.0;
        set => _volumeProvider.Volume = (float)double.Clamp(value * 2, 0.0, 2.0); // map 0-1 to 0-2 to allow amplification.
    }

    public WindowsSfxPlayer()
    {
        var outputFormat = WaveFormat.CreateIeeeFloatWaveFormat(44100, 2);

        _soundProvider = new(outputFormat);
        _volumeProvider = new(_soundProvider) { Volume = 1f };

        _outputDevice = new WasapiOut(AudioClientShareMode.Shared, 50);
        _outputDevice.Init(_volumeProvider);
        _outputDevice.Play();
    }

    public void Unload()
    {
        _soundProvider.SetAudioSource(null);
    }

    public async Task LoadFromFile(string filePath)
    {
        // Use a CancellationToken to gracefully handle disposal while loading.
        using var cts = new CancellationTokenSource();
        if (_disposed) return;

        await using var fileStream = File.OpenRead(filePath);
        await LoadFromStreamAsync(fileStream, Path.GetExtension(filePath), cts.Token);
    }

    /// <param name="resourceUri">The Avalonia resource URI.</param>
    /// <exception cref="ArgumentException">Thrown if the resource does not exist.</exception>
    /// <exception cref="InvalidOperationException">Thrown for unsupported file formats.</exception>
    public async Task LoadFromResource(Uri resourceUri)
    {
        // Use a CancellationToken to gracefully handle disposal while loading.
        using var cts = new CancellationTokenSource();
        if (_disposed) return;

        try
        {
            await using var resourceStream = AssetLoader.Open(resourceUri);
            await LoadFromStreamAsync(resourceStream, Path.GetExtension(resourceUri.AbsolutePath), cts.Token);
        }
        catch (Exception ex) // AssetLoader can throw various exceptions on failure
        {
            throw new ArgumentException($"Failed to load Avalonia resource: {resourceUri}", ex);
        }
    }

    private async Task LoadFromStreamAsync(Stream sourceStream, string audioFormat, CancellationToken cancellationToken)
    {
        await using var memoryStream = new MemoryStream();
        await sourceStream.CopyToAsync(memoryStream, cancellationToken);
        memoryStream.Position = 0;

        await using var reader = CreateReader(memoryStream, audioFormat);

        // The audio is converted to the output device's format and stored as float samples.
        var resampler = new WdlResamplingSampleProvider(reader.ToSampleProvider(), _soundProvider.WaveFormat.SampleRate);

        // Handle channel differences
        ISampleProvider finalProvider = resampler.WaveFormat.Channels switch
        {
            1 when _soundProvider.WaveFormat.Channels == 2 => new MonoToStereoSampleProvider(resampler),
            2 when _soundProvider.WaveFormat.Channels == 1 => new StereoToMonoSampleProvider(resampler),
            _ => resampler,
        };

        // Read from the provider chain directly into our final buffer.
        var samplesCount = (long)(reader.TotalTime.TotalSeconds * finalProvider.WaveFormat.SampleRate) * finalProvider.WaveFormat.Channels;
        var samples = new float[samplesCount];

        int position = 0;
        int samplesRead;
        while ((samplesRead = finalProvider.Read(samples, position, samples.Length - position)) > 0)
        {
            position += samplesRead;
        }

        _soundProvider.SetAudioSource(samples);
    }

    private static WaveStream CreateReader(Stream sourceStream, string audioFormat)
    {
        if (".wav".Equals(audioFormat, StringComparison.OrdinalIgnoreCase))
        {
            return new WaveFileReader(sourceStream);
        }

        throw new NotSupportedException($"Unsupported audio file format: {audioFormat}");
    }

    /// <summary>
    /// Plays the cached sound effect from the beginning. If the sound is already playing, it will be restarted.
    /// </summary>
    public void Play()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        _soundProvider.RestartPlayback();
    }

    /// <summary>
    /// Stops the currently playing sound effect and resets its position to the beginning.
    /// </summary>
    public void Stop()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        _soundProvider.StopPlayback();
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _outputDevice.Stop();
        _outputDevice.Dispose();

        _soundProvider.SetAudioSource(null);
    }

    private sealed class SingleSoundSampleProvider(WaveFormat waveFormat) : ISampleProvider
    {
        private float[]? _sourceSamples;
        private int _position;
        private bool _isPlaying;
        private readonly Lock _lockObject = new();

        public WaveFormat WaveFormat { get; } = waveFormat;

        public void SetAudioSource(float[]? sourceSamples)
        {
            lock (_lockObject)
            {
                _sourceSamples = sourceSamples;
                _position = 0;
                _isPlaying = false;
            }
        }

        public void RestartPlayback()
        {
            if (_sourceSamples is null) return;
            lock (_lockObject)
            {
                if (_sourceSamples is null) return;
                _position = 0;
                _isPlaying = true;
            }
        }

        public void StopPlayback()
        {
            if (!_isPlaying) return;
            lock (_lockObject)
            {
                _isPlaying = false;
                _position = 0;
            }
        }

        public int Read(float[] buffer, int offset, int count)
        {
            lock (_lockObject)
            {
                if (!_isPlaying || _sourceSamples is null)
                {
                    // No sound loaded, output silence.
                    buffer.AsSpan(offset, count).Clear();
                    return count;
                }

                var remainingSamples = _sourceSamples.Length - _position;
                var samplesToCopy = int.Min(count, remainingSamples);

                if (samplesToCopy > 0)
                {
                    _sourceSamples.AsSpan(_position, samplesToCopy).CopyTo(buffer.AsSpan(offset, samplesToCopy));
                    _position += samplesToCopy;
                }

                // If the sound has finished during this read, fill the rest of the buffer with silence.
                if (samplesToCopy < count)
                {
                    _isPlaying = false;
                    buffer.AsSpan(offset + samplesToCopy, count - samplesToCopy).Clear();
                }
            }

            return count;
        }
    }
}
