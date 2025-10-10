using System;
using System.ComponentModel;
using MicMuter.AppSettings;
using MicMuter.Audio;
using Task = System.Threading.Tasks.Task;

namespace MicMuter.MiscServices;

internal sealed class MuteSfxPlayer
{
    private readonly MicMuterService _micMuterService;
    private readonly ISfxPlayer _muteSfxPlayer;
    private readonly ISfxPlayer _unmuteSfxPlayer;

    private readonly Settings _settings;

    public MuteSfxPlayer(MicMuterService micMuterService, Func<ISfxPlayer> sfxPlayerFactory, Settings settings)
    {
        _muteSfxPlayer = sfxPlayerFactory();
        _unmuteSfxPlayer = sfxPlayerFactory();
        _micMuterService = micMuterService;
        _settings = settings;
        Load();
    }

    private async void Load()
    {
        try
        {
            await Task.WhenAll(
                LoadSfx(_settings.MuteSfx.Path, _muteSfxPlayer),
                LoadSfx(_settings.UnmuteSfx.Path, _unmuteSfxPlayer)
            );

            _settings.PropertyChanged += Settings_OnPropertyChanged;

            _micMuterService.MuteStatusChanged += MicMuterService_OnMuteStatusChanged;
        }
        catch (Exception ex)
        {
            Program.OnUnhandledException(ex);
        }
    }

    private void Settings_OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (nameof(Settings.MuteSfx).Equals(e.PropertyName, StringComparison.Ordinal))
        {
            LoadSfx(_settings.MuteSfx.Path, _muteSfxPlayer);
        }
        else if (nameof(Settings.UnmuteSfx).Equals(e.PropertyName, StringComparison.Ordinal))
        {
            LoadSfx(_settings.UnmuteSfx.Path, _unmuteSfxPlayer);
        }
        else if (nameof(Settings.SfxVolume).Equals(e.PropertyName, StringComparison.Ordinal))
        {
            var vol = _settings.SfxVolume / 100.0;
            _muteSfxPlayer.Volume = vol;
            _unmuteSfxPlayer.Volume = vol;
        }
    }

    private static Task LoadSfx(string path, ISfxPlayer player)
    {
        if (path == "")
        {
            player.Unload();
            return Task.CompletedTask;
        }

        if (path.StartsWith("avares:", StringComparison.Ordinal))
        {
            return player.LoadFromResource(new(path));
        }

        return player.LoadFromFile(path);
    }

    private void MicMuterService_OnMuteStatusChanged(object? sender, bool isMuted)
    {
        if (isMuted)
        {
            _unmuteSfxPlayer.Stop();
            _muteSfxPlayer.Play();
        }
        else
        {
            _muteSfxPlayer.Stop();
            _unmuteSfxPlayer.Play();
        }
    }
}
