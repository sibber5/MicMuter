using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Platform;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace MicMuter.Audio.Windows;

/// <summary>
/// Manages loading, caching, and low-latency playback of a single sound effect.
/// It uses a long-lived WasapiOut instance and a mixer to play sounds with minimal latency and garbage collection,
/// making it ideal for frequent playback. Audio is resampled and cached in a uniform format on load.
/// This version uses an object pool for its sample providers to achieve zero allocations in the Play() method.
/// </summary>
public sealed class WindowsSfxPlayer : ISfxPlayer, IDisposable
{
    // Optimization: The audio data is now cached as decoded float samples in the mixer's format.
    // This avoids all on-the-fly conversion during playback.
    private float[]? _cachedAudioSamples;
    private WaveFormat? _cachedWaveFormat; // This will be the format of the mixer
    private bool _isDisposed;

    // Optimization: A single output device and mixer are created and kept alive.
    // This opens the audio device once, keeping it ready for immediate playback and
    // eliminating the overhead of device initialization on every Play() call.
    private readonly WasapiOut _outputDevice;
    private readonly MixingSampleProvider _mixer;

    // Optimization: An object pool for the sample providers to avoid allocations on Play().
    private readonly ConcurrentBag<PooledCachedSoundSampleProvider> _providerPool = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="WindowsSfxPlayer"/> class.
    /// Sets up the audio playback pipeline.
    /// </summary>
    public WindowsSfxPlayer()
    {
        // The mixer format determines the final output format. We choose a standard high-quality format.
        var mixerFormat = WaveFormat.CreateIeeeFloatWaveFormat(44100, 2); // Stereo, 44.1kHz
        _mixer = new MixingSampleProvider(mixerFormat)
        {
            ReadFully = true, // Ensures the mixer continues to provide data even when no sounds are playing.
        };

        // We use a reasonably low latency. 50ms is a good starting point.
        _outputDevice = new WasapiOut(AudioClientShareMode.Shared, 50);
        _outputDevice.Init(_mixer);
        _outputDevice.Play();
    }

    public void Unload()
    {
        _cachedAudioSamples = null;
        _cachedWaveFormat = null;
    }

    /// <summary>
    /// Asynchronously loads a sound effect from a specified file path (.wav or .mp3).
    /// The decoded PCM audio data is cached in memory for efficient, repeated playback.
    /// </summary>
    /// <param name="filePath">The absolute path to the audio file.</param>
    /// <exception cref="FileNotFoundException">Thrown if the audio file does not exist.</exception>
    /// <exception cref="InvalidOperationException">Thrown for unsupported file formats.</exception>
    public async Task LoadFromFile(string filePath)
    {
        // Use a CancellationToken to gracefully handle disposal while loading.
        using var cts = new CancellationTokenSource();
        if (_isDisposed) return;

        await using var fileStream = File.OpenRead(filePath);
        await LoadFromStream(fileStream, Path.GetExtension(filePath), cts.Token);
    }

    /// <summary>
    /// Asynchronously loads a sound effect from an embedded Avalonia resource URI.
    /// The decoded PCM audio data is cached in memory for efficient, repeated playback.
    /// Example URI: "avares://YourAppName/Assets/Sounds/effect.wav"
    /// </summary>
    /// <param name="resourceUri">The Avalonia resource URI.</param>
    /// <exception cref="ArgumentException">Thrown if the resource does not exist.</exception>
    /// <exception cref="InvalidOperationException">Thrown for unsupported file formats.</exception>
    public async Task LoadFromResource(Uri resourceUri)
    {
        // Use a CancellationToken to gracefully handle disposal while loading.
        using var cts = new CancellationTokenSource();
        if (_isDisposed) return;

        try
        {
            await using var resourceStream = AssetLoader.Open(resourceUri);
            await LoadFromStream(resourceStream, Path.GetExtension(resourceUri.AbsolutePath), cts.Token);
        }
        catch (Exception ex) // AssetLoader can throw various exceptions on failure
        {
            throw new ArgumentException($"Failed to load Avalonia resource: {resourceUri}", ex);
        }
    }

    private async Task LoadFromStream(Stream sourceStream, string fileExtension, CancellationToken cancellationToken)
    {
        await using var memoryStream = new MemoryStream();
        await sourceStream.CopyToAsync(memoryStream, cancellationToken);
        memoryStream.Position = 0;

        await using var reader = CreateReader(memoryStream, fileExtension);

        // The audio is converted to the mixer's format and stored as float samples.
        // This pre-processing means the Play() method does zero conversion work.
        _cachedWaveFormat = _mixer.WaveFormat;
        var resampler = new WdlResamplingSampleProvider(reader.ToSampleProvider(), _cachedWaveFormat.SampleRate);

        // Handle channel differences
        ISampleProvider finalProvider = resampler.WaveFormat.Channels switch
        {
            1 when _cachedWaveFormat.Channels == 2 => new MonoToStereoSampleProvider(resampler),
            2 when _cachedWaveFormat.Channels == 1 => new StereoToMonoSampleProvider(resampler),
            _ => resampler,
        };

        // Read all audio data into a buffer
        var allSamples = new List<float>((int)(reader.Length / 4));
        var readBuffer = new float[finalProvider.WaveFormat.SampleRate * finalProvider.WaveFormat.Channels];
        int samplesRead;
        while ((samplesRead = finalProvider.Read(readBuffer, 0, readBuffer.Length)) > 0)
        {
            allSamples.AddRange(readBuffer.Take(samplesRead));
        }

        _cachedAudioSamples = allSamples.ToArray();
    }

    private static WaveStream CreateReader(Stream sourceStream, string fileExtension)
    {
        return fileExtension.ToLowerInvariant() switch
        {
            ".wav" => new WaveFileReader(sourceStream),
            // ".mp3" => new Mp3FileReader(sourceStream),
            _ => throw new InvalidOperationException($"Unsupported audio file format: {fileExtension}"),
        };
    }

    /// <summary>
    /// Plays the cached sound effect. This method is optimized for low latency and can be called frequently.
    /// If another sound is already playing from a previous call, this will start a new playback that overlaps.
    /// This method is allocation-free (amortized).
    /// </summary>
    public void Play()
    {
        if (_isDisposed)
        {
            throw new ObjectDisposedException(nameof(WindowsSfxPlayer));
        }

        // Correctness: Check if audio has been loaded.
        if (_cachedAudioSamples == null || _cachedWaveFormat == null)
        {
            return;
        }

        // Object Pooling: Try to reuse a provider from the pool.
        if (!_providerPool.TryTake(out var provider))
        {
            // If the pool is empty, create a new provider. The pool will grow as needed.
            provider = new PooledCachedSoundSampleProvider(_providerPool);
        }

        // Initialize the provider with the current sound and add it to the mixer.
        provider.Initialize(_cachedAudioSamples, _cachedWaveFormat);
        _mixer.AddMixerInput(provider);
    }

    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;

        // Cleanly shut down the audio device.
        _outputDevice.Stop();
        _outputDevice.Dispose();

        // Clear the pool.
        while (_providerPool.TryTake(out _)) { }

        Unload();
    }

    /// <summary>
    /// A lightweight, reusable, forward-only ISampleProvider that reads from a cached float array.
    /// When it finishes reading, it automatically returns itself to an object pool.
    /// </summary>
    private sealed class PooledCachedSoundSampleProvider(ConcurrentBag<PooledCachedSoundSampleProvider> pool) : ISampleProvider
    {
        private float[]? _sourceSamples;
        private int _position;

        // Start with a dummy format; it will be replaced by Initialize.
        public WaveFormat WaveFormat { get; private set; } = WaveFormat.CreateIeeeFloatWaveFormat(1, 1);

        /// <summary>
        /// Resets the provider with new audio data to be played.
        /// </summary>
        public void Initialize(float[] sourceSamples, WaveFormat waveFormat)
        {
            _sourceSamples = sourceSamples;
            WaveFormat = waveFormat;
            _position = 0;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            if (_sourceSamples == null)
            {
                return 0; // Already finished
            }

            var remainingSamples = _sourceSamples.Length - _position;
            if (remainingSamples <= 0)
            {
                // This is the call where we signal the end.
                _sourceSamples = null; // De-initialize state.
                pool.Add(this);        // Return to pool for reuse.
                return 0;              // Signal end to the mixer.
            }

            var samplesToCopy = int.Min(count, remainingSamples);

            if (samplesToCopy > 0)
            {
                _sourceSamples.AsSpan(_position, samplesToCopy).CopyTo(buffer.AsSpan(offset, samplesToCopy));
                _position += samplesToCopy;
            }

            return samplesToCopy;
        }
    }
}
