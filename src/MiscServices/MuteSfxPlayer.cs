using System;
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

            _settings.PropertyChanged += (_, args) =>
            {
                if (nameof(Settings.MuteSfx).Equals(args.PropertyName, StringComparison.Ordinal))
                {
                    LoadSfx(_settings.MuteSfx.Path, _muteSfxPlayer);
                }
                else if (nameof(Settings.UnmuteSfx).Equals(args.PropertyName, StringComparison.Ordinal))
                {
                    LoadSfx(_settings.UnmuteSfx.Path, _unmuteSfxPlayer);
                }
            };

            _micMuterService.MuteStatusChanged += MicMuterService_OnMuteStatusChanged;
        }
        catch (Exception ex)
        {
            Program.OnUnhandledException(ex);
        }

        static Task LoadSfx(string path, ISfxPlayer player)
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
    }

    private void MicMuterService_OnMuteStatusChanged(object? sender, bool isMuted)
    {
        if (isMuted) _muteSfxPlayer.Play();
        else _unmuteSfxPlayer.Play();
    }
}
