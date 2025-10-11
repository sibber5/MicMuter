using System;
using System.ComponentModel;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using MicMuter.Audio;
using MicMuter.Hotkeys;
using Microsoft.Extensions.Logging;

namespace MicMuter.AppSettings;

public sealed partial class SettingsSerializer
{
    public bool IsLoadingSettings { get; private set; }

    private readonly Settings _settings;
    private readonly IMicDeviceManager _micDeviceManager;
    private readonly ILogger<SettingsSerializer> _logger;

    private readonly SemaphoreSlim _serializeSemaphore = new(1, 1);
    private readonly Timer _debounceTimer;
    private readonly Lock _serializeLock = new();
    private const int DebounceDelayMS = 300;

    public SettingsSerializer(Settings settings, IMicDeviceManager micDeviceManager, ILogger<SettingsSerializer> logger)
    {
        _settings = settings;
        _micDeviceManager = micDeviceManager;
        _logger = logger;
        _debounceTimer = new(DebounceSerialize, null, Timeout.Infinite, Timeout.Infinite);
    }

    private void Settings_OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        lock (_serializeLock)
        {
            _debounceTimer.Change(DebounceDelayMS, Timeout.Infinite);
        }
    }

    private async void DebounceSerialize(object? state)
    {
        try
        {
            await Serialize().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error serializing settings");
            Program.OnUnhandledException(ex);
        }
    }

    private async Task Serialize(CancellationToken token = default)
    {
        await _serializeSemaphore.WaitAsync(token).ConfigureAwait(false);
        try
        {
            Directory.CreateDirectory(Paths.SaveFileDir);

            // the settings file is small, async IO will likely be slower.
            await using FileStream file = new(
                Paths.SaveFilePath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None
            );

            await JsonSerializer.SerializeAsync(file, _settings.ToSettingsDto(), SourceGenerationContext.Default.SettingsDto, token).ConfigureAwait(false);

            file.Flush(true);
            _logger.LogInformation("Successfully Saved settings");
        }
        finally
        {
            _serializeSemaphore.Release();
        }
    }

    public async Task<Settings> Load()
    {
        IsLoadingSettings = true;
        try
        {
            SettingsDto? dto;
            try
            {
                Directory.CreateDirectory(Paths.SaveFileDir);
                await using FileStream readStream = File.OpenRead(Paths.SaveFilePath);
                dto = await JsonSerializer.DeserializeAsync(readStream, SourceGenerationContext.Default.SettingsDto);
            }
            catch (FileNotFoundException)
            {
                _logger.LogWarning("Settings file not found");
                dto = null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected exception while deserializing settings file");
                dto = null;
            }

            dto.LoadInto(_settings, _micDeviceManager);
            // we dont want to listen to prop changes before settings are loaded because that might trigger
            // a serialization while still loading.
            // also, in practice Load() is called only once at startup, so we subscribe to prop change here,
            // but in case that changes we also unsubscribe to avoid duplicate handlers.
            _settings.PropertyChanged -= Settings_OnPropertyChanged;
            _settings.PropertyChanged += Settings_OnPropertyChanged;
            _logger.LogInformation("Successfully loaded settings");

            return _settings;
        }
        finally
        {
            IsLoadingSettings = false;
        }
    }

    internal sealed record SettingsDto(string? MicId, Shortcut? Shortcut, bool? IgnoreExtraModifiers, string? MuteSfx, string? UnmuteSfx, double? SfxVolume, bool? RunOnStartup, bool? StartElevated, bool? StartMinimized);

    [JsonSourceGenerationOptions(WriteIndented = true, UseStringEnumConverter = true)]
    [JsonSerializable(typeof(SettingsDto))]
    private partial class SourceGenerationContext : JsonSerializerContext;
}

file static class SettingsSerializerExtensions
{
    public static SettingsSerializer.SettingsDto ToSettingsDto(this Settings settings)
    {
        return new(
            settings.MicDevice?.Id,
            settings.MuteShortcut,
            settings.IgnoreExtraModifiers,
            settings.MuteSfx == SfxSelection.DefaultMute ? settings.MuteSfx.DisplayName : settings.MuteSfx.Path,
            settings.UnmuteSfx == SfxSelection.DefaultUnmute ? settings.UnmuteSfx.DisplayName : settings.UnmuteSfx.Path,
            settings.SfxVolume,
            settings.RunOnStartup,
            settings.StartElevated,
            settings.StartMinimized
        );
    }

    public static void LoadInto(this SettingsSerializer.SettingsDto? dto, Settings settings, IMicDeviceManager micDeviceManager)
    {
        if (dto is null) return;

        if (dto.RunOnStartup is not null) settings.RunOnStartup = dto.RunOnStartup.Value;
        if (dto.StartElevated is not null) settings.StartElevated = dto.StartElevated.Value;
        if (dto.StartMinimized is not null) settings.StartMinimized = dto.StartMinimized.Value;
        if (dto.IgnoreExtraModifiers is not null) settings.IgnoreExtraModifiers = dto.IgnoreExtraModifiers.Value;
        if (dto.Shortcut is not null) settings.MuteShortcut = dto.Shortcut.Value;
        settings.MicDevice = dto.MicId is not null ? micDeviceManager.GetMicDeviceById(dto.MicId) : micDeviceManager.GetDefaultMicDevice();
        if (dto.SfxVolume is not null) settings.SfxVolume = dto.SfxVolume.Value;
        if (dto.MuteSfx is not null)
        {
            settings.MuteSfx = dto.MuteSfx.Equals(SfxSelection.DefaultMute.DisplayName, StringComparison.Ordinal)
                ? SfxSelection.DefaultMute
                : (dto.MuteSfx.Equals(SfxSelection.None.Path, StringComparison.Ordinal) ? SfxSelection.None : new(dto.MuteSfx));
        }
        if (dto.UnmuteSfx is not null)
        {
            settings.UnmuteSfx = dto.UnmuteSfx.Equals(SfxSelection.DefaultUnmute.DisplayName, StringComparison.Ordinal)
                ? SfxSelection.DefaultUnmute
                : (dto.UnmuteSfx.Equals(SfxSelection.None.Path, StringComparison.Ordinal) ? SfxSelection.None : new(dto.UnmuteSfx));
        }
    }
}
