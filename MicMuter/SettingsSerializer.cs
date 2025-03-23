using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using MicMuter.Audio;
using MicMuter.Hotkeys;

namespace MicMuter;

internal sealed partial class SettingsSerializer(Settings settings, IMicDeviceManager micDeviceManager)
{
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
            Debug.WriteLine($"\n[{nameof(SettingsSerializer)}] Error serializing settings, Exception: {ex}\n");
            throw;
        }
    }

    private async Task Serialize()
    {
        Directory.CreateDirectory(SaveFileDir);
        SettingsDto dto = new(settings.MicDevice?.Id, settings.MuteShortcut);
        await using FileStream createStream = File.Create(SaveFilePath);
        await JsonSerializer.SerializeAsync(createStream, dto, SourceGenerationContext.Default.SettingsDto);
    }

    public async Task Load()
    {
        SettingsDto dto = default;
        try
        {
            Directory.CreateDirectory(SaveFileDir);
            await using FileStream readStream = File.OpenRead(SaveFilePath);
            dto = await JsonSerializer.DeserializeAsync(readStream, SourceGenerationContext.Default.SettingsDto);
        }
        catch (FileNotFoundException)
        {
            Debug.WriteLine($"[{nameof(SettingsSerializer)}] Settings file not found.");
        }
        
        settings.MuteShortcut = dto.Shortcut;
        settings.MicDevice = dto.MicId is not null ? micDeviceManager.GetMicDeviceById(dto.MicId) : micDeviceManager.GetDefaultMicDevice();
        
        settings.PropertyChanged += Settings_OnPropertyChanged;
        
        Debug.WriteLine($"[{nameof(SettingsSerializer)}] Successfully loaded settings.");
    }

    private readonly record struct SettingsDto(string? MicId, Shortcut Shortcut);

    [JsonSourceGenerationOptions(WriteIndented = true)]
    // [JsonConverter(typeof(JsonStringEnumConverter))]
    [JsonSerializable(typeof(SettingsDto))]
    private partial class SourceGenerationContext : JsonSerializerContext;
}
