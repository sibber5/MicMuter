using System;
using System.ComponentModel;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using MicMuter.Audio;
using MicMuter.Hotkeys;

namespace MicMuter.AppSettings;

public sealed partial class SettingsSerializer(Settings settings, IMicDeviceManager micDeviceManager)
{
    public bool IsLoadingSettings { get; private set; }
    
    private static readonly string SaveFileDir = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "/MicMuter");
    private static readonly string SaveFilePath = Path.Join(SaveFileDir, "/UserSettings.json");

    private async void Settings_OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        try
        {
            await Serialize();
        }
        catch (Exception ex)
        {
            Helpers.DebugWriteLine($"\nError serializing settings, Exception: {ex}\n");
            App.ThrowOnMainThread(ex);
        }
    }

    private async Task Serialize()
    {
        Helpers.DebugWriteLine("Saving settings...");
        Directory.CreateDirectory(SaveFileDir);
        SettingsDto dto = new(settings.MicDevice?.Id, settings.MuteShortcut, settings.RunOnStartup, settings.StartElevated, settings.StartMinimized);
        await using FileStream createStream = File.Create(SaveFilePath);
        await JsonSerializer.SerializeAsync(createStream, dto, SourceGenerationContext.Default.SettingsDto);
    }

    public async Task<Settings> Load()
    {
        IsLoadingSettings = true;
        
        SettingsDto dto = default;
        try
        {
            Directory.CreateDirectory(SaveFileDir);
            await using FileStream readStream = File.OpenRead(SaveFilePath);
            dto = await JsonSerializer.DeserializeAsync(readStream, SourceGenerationContext.Default.SettingsDto);
        }
        catch (FileNotFoundException)
        {
            Helpers.DebugWriteLine("Settings file not found.");
        }
        
        settings.MuteShortcut = dto.Shortcut;
        settings.MicDevice = dto.MicId is not null ? micDeviceManager.GetMicDeviceById(dto.MicId) : micDeviceManager.GetDefaultMicDevice();
        settings.RunOnStartup = dto.RunOnStartup;
        settings.StartElevated = dto.StartElevated;
        settings.StartMinimized = dto.StartMinimized;
        
        settings.PropertyChanged += Settings_OnPropertyChanged;
        
        Helpers.DebugWriteLine("Successfully loaded settings.");

        IsLoadingSettings = false;
        return settings;
    }
    
    private readonly record struct SettingsDto(string? MicId, Shortcut Shortcut, bool RunOnStartup, bool StartElevated, bool StartMinimized);
    
    [JsonSourceGenerationOptions(WriteIndented = true)]
    // [JsonConverter(typeof(JsonStringEnumConverter))]
    [JsonSerializable(typeof(SettingsDto))]
    private partial class SourceGenerationContext : JsonSerializerContext;
}
