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
    
    private bool _loaded = false;

    private async void Settings_OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        try
        {
            await Serialize();
        }
        catch (Exception ex)
        {
            logger.LogError("Error serializing settings, Exception: {Exception}", ex);
            Program.OnUnhandledException(ex);
        }
    }

    private async Task Serialize()
    {
        logger.LogInformation("Saving settings...");
        Directory.CreateDirectory(Paths.SaveFileDir);
        await using FileStream createStream = File.Create(Paths.SaveFilePath);
        await JsonSerializer.SerializeAsync(createStream, settings.ToSettingsDto(), SourceGenerationContext.Default.SettingsDto);
    }

    public async Task<Settings> Load()
    {
        if (Interlocked.Exchange(ref _loaded, true)) throw new InvalidOperationException("Already loaded settings.");
        IsLoadingSettings = true;
        
        SettingsDto? dto = null;
        try
        {
            Directory.CreateDirectory(Paths.SaveFileDir);
            await using FileStream readStream = File.OpenRead(Paths.SaveFilePath);
            dto = await JsonSerializer.DeserializeAsync(readStream, SourceGenerationContext.Default.SettingsDto);
        }
        catch (FileNotFoundException)
        {
            logger.LogWarning("Settings file not found.");
        }
        
        dto.LoadInto(settings, micDeviceManager);
        
        settings.PropertyChanged += Settings_OnPropertyChanged;
        
        logger.LogInformation("Successfully loaded settings.");
        
        IsLoadingSettings = false;
        return settings;
    }
    
    internal readonly record struct SettingsDto(string? MicId, Shortcut Shortcut, bool IgnoreExtraModifiers, bool RunOnStartup, bool StartElevated, bool StartMinimized);
    
    [JsonSourceGenerationOptions(WriteIndented = true, UseStringEnumConverter = true)]
    [JsonSerializable(typeof(SettingsDto))]
    private partial class SourceGenerationContext : JsonSerializerContext;
}

file static class MappingExtensions
{
    public static SettingsSerializer.SettingsDto ToSettingsDto(this Settings settings)
    {
        var (midDevice, muteShortcut, ignoreExtraModifiers, runOnStartup, startElevated, startMinimized) = settings;
        return new(midDevice?.Id, muteShortcut, ignoreExtraModifiers, runOnStartup, startElevated, startMinimized);
    }

    public static void LoadInto(this SettingsSerializer.SettingsDto? dto, Settings settings, IMicDeviceManager micDeviceManager)
    {
        var (micId, shortcut, ignoreExtraModifiers, runOnStartup, startElevated, startMinimized)
            = dto ?? settings.ToSettingsDto();
        
        settings.IgnoreExtraModifiers = ignoreExtraModifiers;
        settings.MuteShortcut = shortcut;
        settings.MicDevice = micId is not null ? micDeviceManager.GetMicDeviceById(micId) : micDeviceManager.GetDefaultMicDevice();
        settings.RunOnStartup = runOnStartup;
        settings.StartElevated = startElevated;
        settings.StartMinimized = startMinimized;
    }
}
