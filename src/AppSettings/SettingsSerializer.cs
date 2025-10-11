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

public sealed partial class SettingsSerializer(Settings settings, IMicDeviceManager micDeviceManager, ILogger<SettingsSerializer> logger)
{
    public bool IsLoadingSettings { get; private set; }

    private CancellationTokenSource? _serializeCts;
    private readonly SemaphoreSlim _serializeSemaphore = new(1, 1);
    private const int DebounceDelayMS = 300;

    private async void Settings_OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        try
        {
            var newCts = new CancellationTokenSource();
            var prevCts = Interlocked.Exchange(ref _serializeCts, newCts);
            prevCts?.Cancel();
            prevCts?.Dispose();

            var token = newCts.Token;
            await Task.Delay(DebounceDelayMS, token).ConfigureAwait(false);
            if (token.IsCancellationRequested) return;

            await Serialize(token).ConfigureAwait(false);

            Interlocked.CompareExchange(ref _serializeCts, null, newCts);
            newCts.Dispose();
        }
        catch (Exception ex)
        {
            if (ex is OperationCanceledException) return;
            logger.LogError(ex, "Error serializing settings");
            Program.OnUnhandledException(ex);
        }
    }

    private async Task Serialize(CancellationToken token)
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

            await JsonSerializer.SerializeAsync(file, settings.ToSettingsDto(), SourceGenerationContext.Default.SettingsDto, token).ConfigureAwait(false);

            file.Flush(true);
            logger.LogInformation("Successfully Saved settings");
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
                logger.LogWarning("Settings file not found");
                dto = null;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected exception while deserializing settings file");
                dto = null;
            }

            dto.LoadInto(settings, micDeviceManager);
            settings.PropertyChanged -= Settings_OnPropertyChanged;
            settings.PropertyChanged += Settings_OnPropertyChanged;
            logger.LogInformation("Successfully loaded settings");

            return settings;
        }
        finally
        {
            IsLoadingSettings = false;
        }
    }

    internal readonly record struct SettingsDto(string? MicId, Shortcut Shortcut, bool IgnoreExtraModifiers, string? MuteSfx, string? UnmuteSfx, double SfxVolume, bool RunOnStartup, bool StartElevated, bool StartMinimized);

    [JsonSourceGenerationOptions(WriteIndented = true, UseStringEnumConverter = true)]
    [JsonSerializable(typeof(SettingsDto))]
    private partial class SourceGenerationContext : JsonSerializerContext;
}

file static class MappingExtensions
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

        var (micId, shortcut, ignoreExtraModifiers, muteSfx, unmuteSfx, sfxVolume, runOnStartup, startElevated, startMinimized)
            = dto.Value;

        settings.RunOnStartup = runOnStartup;
        settings.StartElevated = startElevated;
        settings.StartMinimized = startMinimized;
        settings.IgnoreExtraModifiers = ignoreExtraModifiers;
        settings.MuteShortcut = shortcut;
        settings.MicDevice = micId is not null ? micDeviceManager.GetMicDeviceById(micId) : micDeviceManager.GetDefaultMicDevice();
        settings.SfxVolume = sfxVolume;
        settings.MuteSfx = SfxSelection.DefaultMute.DisplayName.Equals(muteSfx, StringComparison.Ordinal) ? SfxSelection.DefaultMute : (string.IsNullOrEmpty(muteSfx) ? SfxSelection.None : new(muteSfx));
        settings.UnmuteSfx = SfxSelection.DefaultUnmute.DisplayName.Equals(unmuteSfx, StringComparison.Ordinal) ? SfxSelection.DefaultUnmute : (string.IsNullOrEmpty(unmuteSfx) ? SfxSelection.None : new(unmuteSfx));
    }
}
